using FluentValidation;

namespace Application.Commands.Product.Gallery.RemoveProductGalleryImage;

public sealed class RemoveProductGalleryImageCommandValidator : AbstractValidator<RemoveProductGalleryImageCommand>
{
	public RemoveProductGalleryImageCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty();
		RuleFor(x => x.ProductId).NotEmpty();
		RuleFor(x => x.GalleryItemId).NotEmpty();
	}
}
