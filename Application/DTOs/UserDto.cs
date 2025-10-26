namespace Application.DTOs;

/// <summary>
/// DTO для користувача (immutable)
/// </summary>
public record UserDto(
    long Id,
    string Username,
    string Email
);
