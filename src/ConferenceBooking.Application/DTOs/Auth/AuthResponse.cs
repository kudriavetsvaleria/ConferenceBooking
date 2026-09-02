namespace ConferenceBooking.Application.DTOs.Auth;

public record AuthResponse(string Token, int UserId, string Name, string Role);