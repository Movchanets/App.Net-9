namespace Application.DTOs;

/// <summary>
/// DTO для користувача (immutable)
/// </summary>
public record UserDto(
    string Username,
    string Name,
    string Surname,
    string Email,
    string PhoneNumber,
    List<string> Roles
);
