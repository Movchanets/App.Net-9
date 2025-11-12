namespace Application.ViewModels;

public record ChangePasswordVM
{
	public required string CurrentPassword { get; init; }
	public required string NewPassword { get; init; }
}
