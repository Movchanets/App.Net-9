namespace Application.ViewModels;

public record UpdateProfileInfoVM
{
	public string? Name { get; init; }
	public string? Surname { get; init; }
	public string? Username { get; init; }
}
