using ConferenceBooking.Application.DTOs.Bookings;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Domain.Entities;
using ConferenceBooking.Domain.Enums;

namespace ConferenceBooking.Application.Services;

public class BookingManagementService : IBookingManagementService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IServiceRepository _serviceRepository;

    public BookingManagementService(
        IBookingRepository bookingRepository,
        IRoomRepository roomRepository,
        IServiceRepository serviceRepository)
    {
        _bookingRepository = bookingRepository;
        _roomRepository = roomRepository;
        _serviceRepository = serviceRepository;
    }

    public async Task<BookingResponse> CreateAsync(CreateBookingRequest request, int userId)
    {
        if (request.StartTime >= request.EndTime)
            throw new ArgumentException("Час початку має бути раніше за час завершення.");

        if (request.StartTime < DateTime.UtcNow)
            throw new ArgumentException("Не можна забронювати час у минулому.");

        var room = await _roomRepository.GetByIdAsync(request.RoomId);
        if (room is null)
            throw new KeyNotFoundException("Зал не знайдено.");

        var overlapping = await _bookingRepository.GetOverlappingAsync(request.RoomId, request.StartTime, request.EndTime);
        if (overlapping.Count > 0)
            throw new InvalidOperationException("Зал вже заброньовано на цей період.");

        var bookingServices = new List<BookingService>();

        foreach (var item in request.Services)
        {
            var service = await _serviceRepository.GetByIdAsync(item.ServiceId);
            if (service is null)
                throw new KeyNotFoundException($"Послугу з id {item.ServiceId} не знайдено.");

            var alreadyBooked = await _bookingRepository.GetBookedServiceQuantityAsync(
                item.ServiceId, request.StartTime, request.EndTime);

            if (alreadyBooked + item.Quantity > service.TotalQuantity)
                throw new InvalidOperationException($"Недостатньо '{service.Name}' на цей період.");

            bookingServices.Add(new BookingService { ServiceId = item.ServiceId, Quantity = item.Quantity });
        }

        var booking = new Booking
        {
            RoomId = request.RoomId,
            UserId = userId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = BookingStatus.Confirmed
        };

        await _bookingRepository.AddAsync(booking, bookingServices);

        return await MapToResponseAsync(booking);
    }

    public async Task<List<BookingResponse>> GetMyBookingsAsync(int userId)
    {
        var bookings = await _bookingRepository.GetByUserIdAsync(userId);

        var responses = new List<BookingResponse>();
        foreach (var booking in bookings)
            responses.Add(await MapToResponseAsync(booking));

        return responses;
    }

    public async Task<string> CancelAsync(int bookingId, int userId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking is null)
            throw new KeyNotFoundException("Бронь не знайдено.");

        if (booking.UserId != userId)
            throw new UnauthorizedAccessException("Це не ваша бронь.");

        if (booking.Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Бронь вже скасована.");

        if (booking.StartTime - DateTime.UtcNow < TimeSpan.FromHours(48))
            throw new InvalidOperationException("Скасувати бронь можна не пізніше ніж за 48 годин до початку.");

        var wasPaid = booking.Status == BookingStatus.Paid;

        booking.Status = BookingStatus.Cancelled;
        await _bookingRepository.UpdateAsync(booking);

        return wasPaid
            ? "Бронь скасовано. Гроші буде повернено."
            : "Бронь скасовано.";
    }

    public async Task PayAsync(int bookingId, int userId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking is null)
            throw new KeyNotFoundException("Бронь не знайдено.");

        if (booking.UserId != userId)
            throw new UnauthorizedAccessException("Це не ваша бронь.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new InvalidOperationException("Оплатити можна лише підтверджену бронь.");

        booking.Status = BookingStatus.Paid;
        await _bookingRepository.UpdateAsync(booking);
    }

    private async Task<BookingResponse> MapToResponseAsync(Booking booking)
    {
        var room = await _roomRepository.GetByIdAsync(booking.RoomId)
                   ?? throw new InvalidOperationException("Дані зала для брони пошкоджено.");

        var bookingServices = await _bookingRepository.GetBookingServicesAsync(booking.Id);

        var hours = (decimal)(booking.EndTime - booking.StartTime).Ticks / TimeSpan.TicksPerHour;
        var totalPrice = room.PricePerHour * hours;

        var serviceResponses = new List<BookingServiceResponseItem>();
        foreach (var bs in bookingServices)
        {
            var service = await _serviceRepository.GetByIdAsync(bs.ServiceId);
            if (service is null) continue;

            serviceResponses.Add(new BookingServiceResponseItem(service.Id, service.Name, bs.Quantity, service.Price));
            totalPrice += service.Price * bs.Quantity;
        }

        return new BookingResponse(
            booking.Id, room.Id, room.Name, booking.StartTime, booking.EndTime,
            booking.Status.ToString(), Math.Round(totalPrice, 2), serviceResponses);
    }
}