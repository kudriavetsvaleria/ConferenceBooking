using ConferenceBooking.Domain.Entities;

namespace ConferenceBooking.Application.Interfaces;

public interface IBookingRepository : IRepository<Booking>
{
    Task AddAsync(Booking booking, List<BookingService> services);
    Task<List<Booking>> GetByUserIdAsync(int userId);
    Task<List<Booking>> GetOverlappingAsync(int roomId, DateTime start, DateTime end);
    Task UpdateAsync(Booking booking);
    Task<int> GetBookedServiceQuantityAsync(int serviceId, DateTime start, DateTime end);
}