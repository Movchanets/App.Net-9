using System;
using System.Security.Claims;
using Application.Models;

namespace Application.Interfaces;

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(Guid identityUserId);
    Task<string> GenerateRefreshTokenAsync(Guid identityUserId);
    Task<TokenResponse> GenerateTokensAsync(Guid identityUserId);
    bool ValidateAccessToken(string token);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

