using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
	public LoginRequestValidator()
	{
		// Validate presence first, then validate format only when present to avoid duplicate errors
		RuleFor(x => x.Email)
			.NotEmpty().WithMessage("Email is required");

		RuleFor(x => x.Email)
			.EmailAddress().WithMessage("Invalid email format")
			.When(x => !string.IsNullOrWhiteSpace(x.Email));

		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("Password is required")
			.MinimumLength(6).WithMessage("Password must be at least 6 characters");
	}
}
