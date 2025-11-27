namespace Application.DTOs;

public record UpdateProfileInfoDto
{
	public string? Name { get; init; }
	public string? Surname { get; init; }
	public string? Username { get; init; }
}
