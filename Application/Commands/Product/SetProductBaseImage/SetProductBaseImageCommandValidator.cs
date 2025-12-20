using FluentValidation;

namespace Application.Commands.Product.SetProductBaseImage;

public sealed class SetProductBaseImageCommandValidator : AbstractValidator<SetProductBaseImageCommand>
{
	public SetProductBaseImageCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty();
		RuleFor(x => x.ProductId).NotEmpty();
		RuleFor(x => x.BaseImageUrl).MaximumLength(500);
	}
}
