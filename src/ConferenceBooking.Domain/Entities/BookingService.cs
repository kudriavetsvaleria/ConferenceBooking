namespace ConferenceBooking.Domain.Entities;

public class BookingService
{
    public int BookingId { get; set; }
    public int ServiceId { get; set; }
    public int Quantity { get; set; }
    public Booking Booking { get; set; } = null!;
}