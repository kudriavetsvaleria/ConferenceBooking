using ConferenceBooking.Application.DTOs.Auth;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Domain.Entities;
using ConferenceBooking.Domain.Enums;

namespace ConferenceBooking.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email);
        if (existing is not null)
            throw new InvalidOperationException("Користувач з такою електронною поштою вже зареєстрований.");

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = UserRole.Client
        };

        await _userRepository.AddAsync(user);

        var token = _tokenService.GenerateToken(user);
        return new AuthResponse(token, user.Id, user.Name, user.Role.ToString());
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Невірний email або пароль.");

        var token = _tokenService.GenerateToken(user);
        return new AuthResponse(token, user.Id, user.Name, user.Role.ToString());
    }
}