using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
	public UpdateUserDtoValidator()
	{
		RuleFor(x => x.Email)
			.EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
			.WithMessage("Invalid email format");

		RuleFor(x => x.Name).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Name));
		RuleFor(x => x.Surname).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Surname));

	}
}
