using Application.Commands.User.Profile.UpdateEmail;
using FluentValidation;

namespace Application.Validators;

public class UpdateEmailCommandValidator : AbstractValidator<UpdateEmailCommand>
{
	public UpdateEmailCommandValidator()
	{
		RuleFor(x => x.Data.Email)
			.NotEmpty().WithMessage("Email is required")
			.EmailAddress();
	}
}
