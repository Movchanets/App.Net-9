namespace Application.ViewModels;

/// <summary>
/// ViewModel для реєстрації користувача
/// Record з init properties для зручного API binding та тестування
/// </summary>
public record RegistrationVM
{
    public required string Email { get; init; }
    public required string Name { get; init; }
    public required string Surname { get; init; }
    public required string Password { get; init; }
    public required string ConfirmPassword { get; init; }
    public string? TurnstileToken { get; init; }
}
