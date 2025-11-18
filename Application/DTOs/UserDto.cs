namespace Application.DTOs;

/// <summary>
/// DTO для користувача (immutable)
/// </summary>
public record UserDto(
    System.Guid Id,
    string Username,
    string Name,
    string Surname,
    string Email,
    string PhoneNumber,
    List<string> Roles,
    string? AvatarUrl = null
);
