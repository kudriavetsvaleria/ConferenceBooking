using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Domain.Entities;
using ConferenceBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConferenceBooking.Infrastructure.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _context;

    public RoomRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Room?> GetByIdAsync(int id)
    {
        return await _context.Rooms.FindAsync(id);
    }

    public async Task AddAsync(Room entity)
    {
        await _context.Rooms.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Room>> SearchAsync(int? minCapacity, DateTime? from, DateTime? to, decimal? maxPrice)
    {
        var query = _context.Rooms.AsQueryable();

        if (minCapacity is not null)
            query = query.Where(r => r.Capacity >= minCapacity);

        if (maxPrice is not null)
            query = query.Where(r => r.PricePerHour <= maxPrice);

        if (from is not null && to is not null)
        {
            query = query.Where(r => !_context.Bookings.Any(b =>
                b.RoomId == r.Id &&
                b.Status != Domain.Enums.BookingStatus.Cancelled &&
                b.StartTime < to && from < b.EndTime));
        }

        return await query.ToListAsync();
    }

    public async Task UpdateAsync(Room room)
    {
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room is not null)
        {
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
        }
    }
}