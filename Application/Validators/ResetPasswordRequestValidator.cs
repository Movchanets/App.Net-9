using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
	public ResetPasswordRequestValidator()
	{
		RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required").EmailAddress();
		RuleFor(x => x.Token).NotEmpty().WithMessage("Token is required");
		RuleFor(x => x.NewPassword).NotEmpty().WithMessage("NewPassword is required").MinimumLength(6);
	}
}
