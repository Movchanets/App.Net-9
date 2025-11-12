using Application.ViewModels;
using FluentValidation;

namespace Application.Validators;

public class UpdateProfileVMValidator : AbstractValidator<UpdateProfileVM>
{
	public UpdateProfileVMValidator()
	{
		RuleFor(x => x.Username).Matches("^[a-zA-Z0-9_.-]{3,30}$").When(x => !string.IsNullOrWhiteSpace(x.Username)).WithMessage("Invalid username format");
		RuleFor(x => x.Name).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Name));
		RuleFor(x => x.Surname).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Surname));
		RuleFor(x => x.PhoneNumber).Matches("^[0-9+\\-() ]{7,20}$").When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber)).WithMessage("Invalid phone number format");
	}
}
