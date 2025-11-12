namespace Application.ViewModels;

public record UpdateProfileVM
{
	public string? Name { get; init; }
	public string? Surname { get; init; }
	public string? Username { get; init; }
	public string? PhoneNumber { get; init; }
}
