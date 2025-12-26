using FluentValidation;

namespace Application.Commands.Product.Sku.DeleteSku;

public sealed class DeleteSkuCommandValidator : AbstractValidator<DeleteSkuCommand>
{
	public DeleteSkuCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty();
		RuleFor(x => x.ProductId).NotEmpty();
		RuleFor(x => x.SkuId).NotEmpty();
	}
}
