using ConferenceBooking.Application.DTOs.Rooms;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Domain.Entities;

namespace ConferenceBooking.Application.Services;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;

    public RoomService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<RoomResponse> CreateAsync(RoomRequest request)
    {
        var room = new Room
        {
            Name = request.Name,
            Capacity = request.Capacity,
            PricePerHour = request.PricePerHour
        };

        await _roomRepository.AddAsync(room);

        return ToResponse(room);
    }

    public async Task<RoomResponse?> GetByIdAsync(int id)
    {
        var room = await _roomRepository.GetByIdAsync(id);
        return room is null ? null : ToResponse(room);
    }

    public async Task<List<RoomResponse>> SearchAsync(int? minCapacity, DateTime? from, DateTime? to, decimal? maxPrice)
    {
        var rooms = await _roomRepository.SearchAsync(minCapacity, from, to, maxPrice);
        return rooms.Select(ToResponse).ToList();
    }

    public async Task<RoomResponse> UpdateAsync(int id, RoomRequest request)
    {
        var room = await _roomRepository.GetByIdAsync(id);
        if (room is null)
            throw new KeyNotFoundException("Зал не знайдено.");

        room.Name = request.Name;
        room.Capacity = request.Capacity;
        room.PricePerHour = request.PricePerHour;

        await _roomRepository.UpdateAsync(room);

        return ToResponse(room);
    }

    public async Task DeleteAsync(int id)
    {
        var room = await _roomRepository.GetByIdAsync(id);
        if (room is null)
            throw new KeyNotFoundException("Зал не знайдено.");

        await _roomRepository.DeleteAsync(id);
    }

    private static RoomResponse ToResponse(Room room) =>
        new(room.Id, room.Name, room.Capacity, room.PricePerHour);
}