namespace Application.DTOs;

public record UpdatePhoneDto
{
	public string PhoneNumber { get; init; } = string.Empty;
}
