namespace Application.ViewModels;

public record UpdateUserVM
{
	public string? Name { get; init; }
	public string? Surname { get; init; }
	public string? Email { get; init; }
}
