using Application.Commands.User.Profile.UpdateProfileInfo;
using FluentValidation;

namespace Application.Validators;

public class UpdateProfileInfoCommandValidator : AbstractValidator<UpdateProfileInfoCommand>
{
	public UpdateProfileInfoCommandValidator()
	{
		RuleFor(x => x.Data.Username)
			.Matches("^[a-zA-Z0-9_.-]{3,30}$")
			.When(x => !string.IsNullOrWhiteSpace(x.Data.Username))
			.WithMessage("Invalid username format");

		RuleFor(x => x.Data.Name)
			.MaximumLength(50)
			.When(x => !string.IsNullOrWhiteSpace(x.Data.Name));

		RuleFor(x => x.Data.Surname)
			.MaximumLength(50)
			.When(x => !string.IsNullOrWhiteSpace(x.Data.Surname));
	}
}
