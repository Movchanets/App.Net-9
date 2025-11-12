using Application.ViewModels;
using FluentValidation;

namespace Application.Validators;

public class UpdateUserVMValidator : AbstractValidator<UpdateUserVM>
{
	public UpdateUserVMValidator()
	{
		RuleFor(x => x.Email)
			.EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
			.WithMessage("Invalid email format");

		RuleFor(x => x.Name).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Name));
		RuleFor(x => x.Surname).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Surname));
		RuleFor(x => x.PhoneNumber).Matches("^[0-9+\\-() ]{7,20}$").When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber)).WithMessage("Invalid phone number format");
	}
}
