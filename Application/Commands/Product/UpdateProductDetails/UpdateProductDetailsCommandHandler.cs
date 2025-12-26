using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Product.UpdateProductDetails;

public sealed class UpdateProductDetailsCommandHandler : IRequestHandler<UpdateProductDetailsCommand, ServiceResponse>
{
	private readonly IProductRepository _productRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<UpdateProductDetailsCommandHandler> _logger;

	public UpdateProductDetailsCommandHandler(
		IProductRepository productRepository,
		IUnitOfWork unitOfWork,
		ILogger<UpdateProductDetailsCommandHandler> logger)
	{
		_productRepository = productRepository;
		_unitOfWork = unitOfWork;
		_logger = logger;
	}

	public async Task<ServiceResponse> Handle(UpdateProductDetailsCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Updating product details for product {ProductId} by user {UserId}", request.ProductId, request.UserId);

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

			product.Rename(request.Name);
			product.UpdateDescription(request.Description);

			await _unitOfWork.SaveChangesAsync(cancellationToken);
			return new ServiceResponse(true, "Product details updated successfully");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating product details for product {ProductId}", request.ProductId);
			return new ServiceResponse(false, $"Error: {ex.Message}");
		}
	}
}
