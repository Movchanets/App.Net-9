namespace Application.DTOs;

public record UpdateEmailDto
{
	public string Email { get; init; } = string.Empty;
}
