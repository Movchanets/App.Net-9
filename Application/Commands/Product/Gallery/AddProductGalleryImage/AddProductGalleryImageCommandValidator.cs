using FluentValidation;

namespace Application.Commands.Product.Gallery.AddProductGalleryImage;

public sealed class AddProductGalleryImageCommandValidator : AbstractValidator<AddProductGalleryImageCommand>
{
	public AddProductGalleryImageCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty();
		RuleFor(x => x.ProductId).NotEmpty();
		RuleFor(x => x.MediaImageId).NotEmpty();
		RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
	}
}
