using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
{
	public UpdateProfileDtoValidator()
	{
		RuleFor(x => x.Username).Matches("^[a-zA-Z0-9_.-]{3,30}$").When(x => !string.IsNullOrWhiteSpace(x.Username)).WithMessage("Invalid username format");
		RuleFor(x => x.Name).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Name));
		RuleFor(x => x.Surname).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Surname));

	}
}
