using FluentValidation;

namespace Application.Commands.Product.UpdateProduct;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
	public UpdateProductCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty();
		RuleFor(x => x.ProductId).NotEmpty();
		RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
		RuleFor(x => x.Description).MaximumLength(2000);
		RuleFor(x => x.CategoryIds).NotNull().NotEmpty();
		RuleForEach(x => x.CategoryIds).NotEmpty();
	}
}
