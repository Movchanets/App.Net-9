using System.Security.Claims;
using Infrastructure.Entities;

namespace Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(UserEntity user);
    string GenerateRefreshToken();
    bool ValidateAccessToken(string token);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

