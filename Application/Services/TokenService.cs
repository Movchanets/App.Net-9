using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Interfaces;
using Infrastructure.Data.Models;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services;

/// <summary>
/// Сервіс для роботи з JWT токенами
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly UserManager<UserEntity> _userManager;
    private readonly RoleManager<RoleEntity> _roleManager;
    private readonly IUserClaimsPrincipalFactory<UserEntity> _claimsFactory;

    /// <summary>
    /// Ініціалізує новий екземпляр TokenService
    /// </summary>
    public TokenService(IConfiguration config, UserManager<UserEntity> userManager, RoleManager<RoleEntity> roleManager,
        IUserClaimsPrincipalFactory<UserEntity> claimsFactory)
    {
        _config = config;
        _userManager = userManager;
        _roleManager = roleManager;
        _claimsFactory = claimsFactory;
    }
    private async Task<List<Claim>> GetValidClaims(UserEntity user)
    {
        // Start with claims produced by the registered ClaimsPrincipalFactory (includes Name, NameIdentifier, etc.)
        var principal = await _claimsFactory.CreateAsync(user);
        var claims = principal.Claims.ToList();

        // Ensure we have a JTI
        if (!claims.Any(c => c.Type == JwtRegisteredClaimNames.Jti))
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

        // Add/merge claims stored explicitly for the user
        var userClaims = await _userManager.GetClaimsAsync(user);
        foreach (var uc in userClaims)
        {
            if (!claims.Any(c => c.Type == uc.Type && c.Value == uc.Value))
                claims.Add(uc);
        }

        // Add roles as Role claims and merge role-specific claims
        var userRoles = await _userManager.GetRolesAsync(user);
        foreach (var userRole in userRoles)
        {
            if (!claims.Any(c => c.Type == ClaimTypes.Role && c.Value == userRole))
                claims.Add(new Claim(ClaimTypes.Role, userRole));

            

            var role = await _roleManager.FindByNameAsync(userRole);
            if (role != null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                foreach (Claim roleClaim in roleClaims)
                {
                    if (!claims.Any(c => c.Type == roleClaim.Type && c.Value == roleClaim.Value))
                        claims.Add(roleClaim);
                }
            }
        }

       

        return claims;
    }
    /// <summary>
    /// Генерує JWT access token для користувача
    /// </summary>
    /// <param name="user">Користувач для якого генерується токен</param>
    /// <returns>JWT токен</returns>
    public async Task<string> GenerateAccessTokenAsync(UserEntity user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _config["JwtSettings:AccessTokenSecret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims: await GetValidClaims(user),
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["JwtSettings:AccessTokenExpirationMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Генерує refresh token
    /// </summary>
    /// <returns>Refresh token</returns>
    public async Task<string> GenerateRefreshTokenAsync(UserEntity user)
    {
        var newRefreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);
        return    newRefreshToken;    
    }

    public async Task<TokenResponse> GenerateTokensAsync(UserEntity user)
    {
        return new TokenResponse(await GenerateAccessTokenAsync(user), await GenerateRefreshTokenAsync(user));
    }

    /// <summary>
    /// Валідує JWT access token
    /// </summary>
    /// <param name="token">JWT токен для валідації</param>
    /// <returns>true якщо токен валідний, інакше false</returns>
    public bool ValidateAccessToken(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _config["JwtSettings:AccessTokenSecret"]!));

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _config["JwtSettings:Issuer"],
            ValidAudience = _config["JwtSettings:Audience"],
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero // щоб токен не був дійсний ще 5 хвилин після закінчення
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Отримує ClaimsPrincipal з expired токена (для refresh)
    /// </summary>
    /// <param name="token">Expired JWT токен</param>
    /// <returns>ClaimsPrincipal або null якщо не вдалось</returns>
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _config["JwtSettings:AccessTokenSecret"]!));

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateLifetime = false // важливо для expired токенів
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}
