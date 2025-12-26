using FluentValidation;

namespace Application.Commands.Product.Sku.UpdateSku;

public sealed class UpdateSkuCommandValidator : AbstractValidator<UpdateSkuCommand>
{
	public UpdateSkuCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty();
		RuleFor(x => x.ProductId).NotEmpty();
		RuleFor(x => x.SkuId).NotEmpty();
		RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
		RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
	}
}
