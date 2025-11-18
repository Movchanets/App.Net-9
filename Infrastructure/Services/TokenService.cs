using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Infrastructure.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

/// <summary>
/// Сервіс для роботи з JWT токенами
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<RoleEntity> _roleManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Ініціалізує новий екземпляр TokenService
    /// </summary>
    public TokenService(IConfiguration config, UserManager<ApplicationUser> userManager, RoleManager<RoleEntity> roleManager,
        IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory, IUserRepository userRepository)
    {
        _config = config;
        _userManager = userManager;
        _roleManager = roleManager;
        _claimsFactory = claimsFactory;
        _userRepository = userRepository;
    }
    private async Task<List<Claim>> GetValidClaims(
    ApplicationUser identityUser,
    User domainUser)
    {
        var principal = await _claimsFactory.CreateAsync(identityUser);
        var claims = principal.Claims.ToList();

        // JTI
        if (!claims.Any(c => c.Type == JwtRegisteredClaimNames.Jti))
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        claims.Add(new Claim("firstName", domainUser.Name ?? ""));
        claims.Add(new Claim("lastName", domainUser.Surname ?? "")); // Або family_name
        claims.Add(new Claim("isBlocked", domainUser.IsBlocked.ToString()));
        if (domainUser.Avatar != null)
        {
            claims.Add(new Claim("picture", domainUser.Avatar.StorageKey));
        }

        // Додаємо кастомні клейми, збережені для IdentityUser
        var userClaims = await _userManager.GetClaimsAsync(identityUser);
        foreach (var uc in userClaims)
        {
            if (!claims.Any(c => c.Type == uc.Type && c.Value == uc.Value))
                claims.Add(uc);
        }
        // Додаємо ролі та клейми, що належать цим ролям (все як у вас)
        var userRoles = await _userManager.GetRolesAsync(identityUser);
        foreach (var userRole in userRoles)
        {
            if (!claims.Any(c => c.Type == ClaimTypes.Role && c.Value == userRole))
                claims.Add(new Claim(ClaimTypes.Role, userRole));

            // Додаємо клейми з самої ролі (Permissions)
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
    public async Task<string> GenerateAccessTokenAsync(Guid identityUserId)
    {
        // 1. Отримуємо Identity-користувача
        var identityUser = await _userManager.FindByIdAsync(identityUserId.ToString());
        if (identityUser == null)
            throw new InvalidOperationException("Identity user not found");

        // 2. Отримуємо Domain-користувача (НОВИЙ РЯДОК)
        var domainUser = await _userRepository.GetByIdentityUserIdAsync(identityUserId);
        if (domainUser == null)
            throw new InvalidOperationException("Domain user profile not found for this identity");

        // --- Решта коду без змін ---
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _config["JwtSettings:AccessTokenSecret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],

            claims: await GetValidClaims(identityUser, domainUser),

            expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["JwtSettings:AccessTokenExpirationMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);

    }

    /// <summary>
    /// Генерує refresh token
    /// </summary>
    /// <returns>Refresh token</returns>
    public async Task<string> GenerateRefreshTokenAsync(Guid identityUserId)
    {
        var user = await _userManager.FindByIdAsync(identityUserId.ToString());
        if (user == null) throw new InvalidOperationException("Identity user not found");
        var newRefreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);
        return newRefreshToken;
    }

    public async Task<TokenResponse> GenerateTokensAsync(Guid identityUserId)
    {
        return new TokenResponse(await GenerateAccessTokenAsync(identityUserId), await GenerateRefreshTokenAsync(identityUserId));
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
