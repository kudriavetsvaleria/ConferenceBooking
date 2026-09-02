using ConferenceBooking.Domain.Entities;

namespace ConferenceBooking.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}