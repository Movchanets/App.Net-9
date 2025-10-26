namespace Application.DTOs;

/// <summary>
/// Request для логіну користувача
/// </summary>
public record LoginRequest(
    string Email,
    string Password
);

/// <summary>
/// Request для оновлення токенів
/// </summary>
public record TokenRequest(
    string AccessToken,
    string RefreshToken
);
