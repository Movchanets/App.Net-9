using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Product.Gallery.RemoveProductGalleryImage;

public sealed class RemoveProductGalleryImageCommandHandler : IRequestHandler<RemoveProductGalleryImageCommand, ServiceResponse>
{
	private readonly IProductRepository _productRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<RemoveProductGalleryImageCommandHandler> _logger;

	public RemoveProductGalleryImageCommandHandler(
		IProductRepository productRepository,
		IUnitOfWork unitOfWork,
		ILogger<RemoveProductGalleryImageCommandHandler> logger)
	{
		_productRepository = productRepository;
		_unitOfWork = unitOfWork;
		_logger = logger;
	}

	public async Task<ServiceResponse> Handle(RemoveProductGalleryImageCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Removing gallery item {GalleryItemId} from product {ProductId} by user {UserId}", request.GalleryItemId, request.ProductId, request.UserId);

		try
		{
			var product = await _productRepository.GetByIdAsync(request.ProductId);
			if (product?.Store is null || product.Store.UserId != request.UserId)
			{
				_logger.LogWarning("Product {ProductId} not found for user {UserId}", request.ProductId, request.UserId);
				return new ServiceResponse(false, "Product not found");
			}

			if (product.Store.IsSuspended)
			{
				_logger.LogWarning("Store {StoreId} is suspended", product.Store.Id);
				return new ServiceResponse(false, "Store is suspended");
			}

			var removed = product.RemoveGalleryItem(request.GalleryItemId);
			if (removed is null)
			{
				_logger.LogWarning("Gallery item {GalleryItemId} not found for product {ProductId}", request.GalleryItemId, request.ProductId);
				return new ServiceResponse(false, "Gallery image not found");
			}

			await _unitOfWork.SaveChangesAsync(cancellationToken);
			return new ServiceResponse(true, "Gallery image removed successfully");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error removing gallery item {GalleryItemId} from product {ProductId}", request.GalleryItemId, request.ProductId);
			return new ServiceResponse(false, $"Error: {ex.Message}");
		}
	}
}
