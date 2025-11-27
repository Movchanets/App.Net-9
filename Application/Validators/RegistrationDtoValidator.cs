using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class RegistrationDtoValidator : AbstractValidator<RegistrationDto>
{
	public RegistrationDtoValidator()
	{
		// Validate presence first, then validate format only when present to avoid duplicate errors
		RuleFor(x => x.Email)
			.NotEmpty().WithMessage("Email is required");

		RuleFor(x => x.Email)
			.EmailAddress().WithMessage("Invalid email format")
			.When(x => !string.IsNullOrWhiteSpace(x.Email));
		RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required").MaximumLength(50);
		RuleFor(x => x.Surname).MaximumLength(50);
		RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
		RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("ConfirmPassword must match Password");
	}
}
