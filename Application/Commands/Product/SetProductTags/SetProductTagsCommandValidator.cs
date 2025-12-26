using FluentValidation;

namespace Application.Commands.Product.SetProductTags;

public sealed class SetProductTagsCommandValidator : AbstractValidator<SetProductTagsCommand>
{
	public SetProductTagsCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty();
		RuleFor(x => x.ProductId).NotEmpty();
		RuleFor(x => x.TagIds).NotNull();
		RuleForEach(x => x.TagIds).NotEmpty();
	}
}
