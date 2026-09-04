using ConferenceBooking.Application.DTOs.Bookings;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Application.Services;
using ConferenceBooking.Domain.Entities;
using ConferenceBooking.Domain.Enums;
using Moq;

namespace ConferenceBooking.Tests;

public class BookingManagementServiceTests
{
    private readonly Mock<IBookingRepository> _bookingRepositoryMock = new();
    private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
    private readonly Mock<IServiceRepository> _serviceRepositoryMock = new();
    private readonly BookingManagementService _sut;

    public BookingManagementServiceTests()
    {
        _sut = new BookingManagementService(
            _bookingRepositoryMock.Object,
            _roomRepositoryMock.Object,
            _serviceRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenStartTimeIsAfterEndTime_ThrowsArgumentException()
    {
        var request = new CreateBookingRequest(
            RoomId: 1,
            StartTime: new DateTime(2026, 1, 10, 12, 0, 0),
            EndTime: new DateTime(2026, 1, 10, 10, 0, 0),
            Services: new List<BookingServiceItem>());

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(request, userId: 1));
    }

    [Fact]
    public async Task CreateAsync_WhenRoomDoesNotExist_ThrowsKeyNotFoundException()
    {
        _roomRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Room?)null);

        var request = new CreateBookingRequest(
            RoomId: 1,
            StartTime: DateTime.UtcNow.AddDays(1),
            EndTime: DateTime.UtcNow.AddDays(1).AddHours(2),
            Services: new List<BookingServiceItem>());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CreateAsync(request, userId: 1));
    }

    [Fact]
    public async Task CreateAsync_WhenRoomIsAlreadyBooked_ThrowsInvalidOperationException()
    {
        var room = new Room { Id = 1, Name = "Зал А", Capacity = 10, PricePerHour = 500 };
        _roomRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(room);
        _bookingRepositoryMock
            .Setup(r => r.GetOverlappingAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Booking> { new() { Id = 99, RoomId = 1 } });

        var request = new CreateBookingRequest(
            RoomId: 1,
            StartTime: DateTime.UtcNow.AddDays(1),
            EndTime: DateTime.UtcNow.AddDays(1).AddHours(2),
            Services: new List<BookingServiceItem>());

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(request, userId: 1));
    }

    [Fact]
    public async Task CreateAsync_WhenNotEnoughServiceInventory_ThrowsInvalidOperationException()
    {
        var room = new Room { Id = 1, Name = "Зал А", Capacity = 10, PricePerHour = 500 };
        var service = new Service { Id = 1, Name = "Проектор", Price = 500, TotalQuantity = 2 };

        _roomRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(room);
        _bookingRepositoryMock
            .Setup(r => r.GetOverlappingAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Booking>());
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(service);
        _bookingRepositoryMock
            .Setup(r => r.GetBookedServiceQuantityAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(1); // уже занята 1 единица из 2

        var request = new CreateBookingRequest(
            RoomId: 1,
            StartTime: DateTime.UtcNow.AddDays(1),
            EndTime: DateTime.UtcNow.AddDays(1).AddHours(2),
            Services: new List<BookingServiceItem> { new(ServiceId: 1, Quantity: 2) }); // просим ещё 2 — итого было бы 3 из 2

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(request, userId: 1));
    }

    [Fact]
    public async Task CreateAsync_WhenEverythingIsValid_CalculatesCorrectTotalPrice()
    {
        var room = new Room { Id = 1, Name = "Зал А", Capacity = 10, PricePerHour = 500 };
        var service = new Service { Id = 1, Name = "Проектор", Price = 300, TotalQuantity = 5 };

        _roomRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(room);
        _bookingRepositoryMock
            .Setup(r => r.GetOverlappingAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Booking>());
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(service);
        _bookingRepositoryMock
            .Setup(r => r.GetBookedServiceQuantityAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(0);
        _bookingRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<List<BookingService>>()))
            .Callback<Booking, List<BookingService>>((b, _) => b.Id = 1)
            .Returns(Task.CompletedTask);
        _bookingRepositoryMock
            .Setup(r => r.GetBookingServicesAsync(1))
            .ReturnsAsync(new List<BookingService> { new() { ServiceId = 1, Quantity = 1 } });

        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddHours(2); // 2 часа

        var request = new CreateBookingRequest(
            RoomId: 1,
            StartTime: start,
            EndTime: end,
            Services: new List<BookingServiceItem> { new(ServiceId: 1, Quantity: 1) });

        var result = await _sut.CreateAsync(request, userId: 1);

        // 2 часа * 500 (зал) + 1 * 300 (услуга) = 1300
        Assert.Equal(1300, result.TotalPrice);
        Assert.Equal("Confirmed", result.Status);
    }
}