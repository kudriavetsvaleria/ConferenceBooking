using ConferenceBooking.Domain.Entities;

namespace ConferenceBooking.Application.Interfaces;

public interface IRoomRepository : IRepository<Room>
{
    Task<List<Room>> SearchAsync(int? minCapacity, DateTime? from, DateTime? to, decimal? maxPrice);
    Task UpdateAsync(Room room);
    Task DeleteAsync(int id);
}