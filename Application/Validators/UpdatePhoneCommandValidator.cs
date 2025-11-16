using Application.Commands.User.Profile.UpdatePhone;
using FluentValidation;

namespace Application.Validators;

public class UpdatePhoneCommandValidator : AbstractValidator<UpdatePhoneCommand>
{
	public UpdatePhoneCommandValidator()
	{
		RuleFor(x => x.Data.PhoneNumber)
			.NotEmpty().WithMessage("Phone number is required")
			.Matches(@"^[0-9+\-() ]{7,20}$").WithMessage("Invalid phone number format");
	}
}
