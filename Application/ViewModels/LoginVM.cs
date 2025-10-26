namespace Application.ViewModels;

/// <summary>
/// ViewModel для логіну
/// Record з init properties для зручного API binding
/// </summary>
public record LoginVM
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
