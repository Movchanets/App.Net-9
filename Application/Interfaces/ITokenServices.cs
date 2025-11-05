using System.Security.Claims;
using Infrastructure.Data.Models;
using Infrastructure.Entities;

namespace Application.Interfaces;

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(UserEntity user);
    Task<string> GenerateRefreshTokenAsync(UserEntity user);
    Task<TokenResponse> GenerateTokensAsync(UserEntity user);
    bool ValidateAccessToken(string token);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

