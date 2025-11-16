namespace Application.Models;

/// <summary>
/// ViewModel для користувача з повною інформацією
/// Mutable через init - дозволяє AutoMapper mapping
/// </summary>
public record UserVM
{
    public System.Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Image { get; init; } = string.Empty;
    public bool IsBlocked { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public List<string> UserRoles { get; init; } = new();
}
