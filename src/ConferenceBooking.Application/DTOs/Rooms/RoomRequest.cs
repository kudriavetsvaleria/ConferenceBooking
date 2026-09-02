namespace ConferenceBooking.Application.DTOs.Rooms;

public record RoomRequest(string Name, int Capacity, decimal PricePerHour);