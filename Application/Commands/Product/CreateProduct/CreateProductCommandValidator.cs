using FluentValidation;

namespace Application.Commands.Product.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
	public CreateProductCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty();
		RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
		RuleFor(x => x.Description).MaximumLength(2000);
		RuleFor(x => x.CategoryId).NotEmpty();
		RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
		RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
	}
}
