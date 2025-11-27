namespace Application.DTOs;

public record UpdateUserDto
{
	public string? Name { get; init; }
	public string? Surname { get; init; }
	public string? Email { get; init; }
}
