using ConferenceBooking.Application.DTOs.Rooms;

namespace ConferenceBooking.Application.Interfaces;

public interface IRoomService
{
    Task<RoomResponse> CreateAsync(RoomRequest request);
    Task<RoomResponse?> GetByIdAsync(int id);
    Task<List<RoomResponse>> SearchAsync(int? minCapacity, DateTime? from, DateTime? to, decimal? maxPrice);
    Task<RoomResponse> UpdateAsync(int id, RoomRequest request);
    Task DeleteAsync(int id);
}