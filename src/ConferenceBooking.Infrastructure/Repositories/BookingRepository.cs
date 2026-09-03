using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Domain.Entities;
using ConferenceBooking.Domain.Enums;
using ConferenceBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConferenceBooking.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;

    public BookingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetByIdAsync(int id)
    {
        return await _context.Bookings.FindAsync(id);
    }

    public async Task AddAsync(Booking booking)
    {
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();
    }

    public async Task AddAsync(Booking booking, List<BookingService> services)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            await AddAsync(booking);

            foreach (var service in services)
            {
                service.BookingId = booking.Id;
            }

            await _context.BookingServices.AddRangeAsync(services);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<Booking>> GetByUserIdAsync(int userId)
    {
        return await _context.Bookings
            .Where(b => b.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<Booking>> GetOverlappingAsync(int roomId, DateTime start, DateTime end)
    {
        return await _context.Bookings
            .Where(b => b.RoomId == roomId
                        && b.Status != BookingStatus.Cancelled
                        && b.StartTime < end && start < b.EndTime)
            .ToListAsync();
    }

    public async Task UpdateAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetBookedServiceQuantityAsync(int serviceId, DateTime start, DateTime end)
    {
        return await _context.BookingServices
            .Where(bs => bs.ServiceId == serviceId
                         && bs.Booking.Status != BookingStatus.Cancelled
                         && bs.Booking.StartTime < end && start < bs.Booking.EndTime)
            .SumAsync(bs => bs.Quantity);
    }
}