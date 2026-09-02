namespace ConferenceBooking.Application.DTOs.Bookings;

public record BookingServiceResponseItem(int ServiceId, string ServiceName, int Quantity, decimal Price);