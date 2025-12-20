using FluentValidation;

namespace Application.Commands.Product.SetProductCategories;

public sealed class SetProductCategoriesCommandValidator : AbstractValidator<SetProductCategoriesCommand>
{
	public SetProductCategoriesCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty();
		RuleFor(x => x.ProductId).NotEmpty();
		RuleFor(x => x.CategoryIds).NotNull();
		RuleForEach(x => x.CategoryIds).NotEmpty();
	}
}
