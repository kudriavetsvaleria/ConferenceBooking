namespace ConferenceBooking.Application.DTOs.Rooms;

public record RoomResponse(int Id, string Name, int Capacity, decimal PricePerHour);