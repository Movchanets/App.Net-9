using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class TokenRequestValidator : AbstractValidator<TokenRequest>
{
	public TokenRequestValidator()
	{
		RuleFor(x => x.AccessToken).NotEmpty().WithMessage("AccessToken is required");
		RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("RefreshToken is required");
	}
}
