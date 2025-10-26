namespace Infrastructure.Data.Models;

/// <summary>
/// Response з токенами (immutable)
/// </summary>
public sealed record TokenResponse(
    string AccessToken,
    string RefreshToken
);
