using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Product.Gallery.AddProductGalleryImage;

public sealed class AddProductGalleryImageCommandHandler : IRequestHandler<AddProductGalleryImageCommand, ServiceResponse<Guid>>
{
	private readonly IProductRepository _productRepository;
	private readonly IMediaImageRepository _mediaImageRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<AddProductGalleryImageCommandHandler> _logger;

	public AddProductGalleryImageCommandHandler(
		IProductRepository productRepository,
		IMediaImageRepository mediaImageRepository,
		IUnitOfWork unitOfWork,
		ILogger<AddProductGalleryImageCommandHandler> logger)
	{
		_productRepository = productRepository;
		_mediaImageRepository = mediaImageRepository;
		_unitOfWork = unitOfWork;
		_logger = logger;
	}

	public async Task<ServiceResponse<Guid>> Handle(AddProductGalleryImageCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Adding gallery image {MediaImageId} to product {ProductId} by user {UserId}", request.MediaImageId, request.ProductId, request.UserId);

		try
		{
			var product = await _productRepository.GetByIdAsync(request.ProductId);
			if (product?.Store is null || product.Store.UserId != request.UserId)
			{
				_logger.LogWarning("Product {ProductId} not found for user {UserId}", request.ProductId, request.UserId);
				return new ServiceResponse<Guid>(false, "Product not found");
			}

			if (product.Store.IsSuspended)
			{
				_logger.LogWarning("Store {StoreId} is suspended", product.Store.Id);
				return new ServiceResponse<Guid>(false, "Store is suspended");
			}

			var media = await _mediaImageRepository.GetByIdAsync(request.MediaImageId);
			if (media is null)
			{
				_logger.LogWarning("MediaImage {MediaImageId} not found", request.MediaImageId);
				return new ServiceResponse<Guid>(false, "Image not found");
			}

			product.AddGalleryItem(media, request.DisplayOrder);

			await _unitOfWork.SaveChangesAsync(cancellationToken);

			var galleryId = product.Gallery
				.Where(g => g.MediaImageId == request.MediaImageId && g.DisplayOrder == request.DisplayOrder)
				.OrderByDescending(g => g.CreatedAt)
				.Select(g => g.Id)
				.FirstOrDefault();

			return new ServiceResponse<Guid>(true, "Gallery image added successfully", galleryId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding gallery image {MediaImageId} to product {ProductId}", request.MediaImageId, request.ProductId);
			return new ServiceResponse<Guid>(false, $"Error: {ex.Message}");
		}
	}
}
