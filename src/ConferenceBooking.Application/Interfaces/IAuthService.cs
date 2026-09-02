using ConferenceBooking.Application.DTOs.Auth;

namespace ConferenceBooking.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}