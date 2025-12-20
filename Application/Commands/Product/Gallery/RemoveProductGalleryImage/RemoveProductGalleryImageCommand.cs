using Application.DTOs;
using MediatR;

namespace Application.Commands.Product.Gallery.RemoveProductGalleryImage;

public sealed record RemoveProductGalleryImageCommand(
	Guid UserId,
	Guid ProductId,
	Guid GalleryItemId
) : IRequest<ServiceResponse>;
