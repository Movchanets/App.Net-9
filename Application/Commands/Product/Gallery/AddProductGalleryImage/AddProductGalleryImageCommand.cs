using Application.DTOs;
using MediatR;

namespace Application.Commands.Product.Gallery.AddProductGalleryImage;

public sealed record AddProductGalleryImageCommand(
	Guid UserId,
	Guid ProductId,
	Guid MediaImageId,
	int DisplayOrder = 0
) : IRequest<ServiceResponse<Guid>>;
