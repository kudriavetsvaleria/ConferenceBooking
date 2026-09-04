using ConferenceBooking.Application.DTOs.Bookings;

namespace ConferenceBooking.Application.Interfaces;

public interface IBookingManagementService
{
    Task<BookingResponse> CreateAsync(CreateBookingRequest request, int userId);
    Task<List<BookingResponse>> GetMyBookingsAsync(int userId);
    Task<string> CancelAsync(int bookingId, int userId);
    Task PayAsync(int bookingId, int userId);
}