using FluentValidation;

namespace Application.Commands.Product.Sku.AddSkuToProduct;

public sealed class AddSkuToProductCommandValidator : AbstractValidator<AddSkuToProductCommand>
{
	public AddSkuToProductCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty();
		RuleFor(x => x.ProductId).NotEmpty();
		RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
		RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
	}
}
