using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Product.DeleteProduct;

public sealed class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, ServiceResponse>
{
	private readonly IProductRepository _productRepository;
	private readonly IStoreRepository _storeRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<DeleteProductCommandHandler> _logger;

	public DeleteProductCommandHandler(
		IProductRepository productRepository,
		IStoreRepository storeRepository,
		IUnitOfWork unitOfWork,
		ILogger<DeleteProductCommandHandler> logger)
	{
		_productRepository = productRepository;
		_storeRepository = storeRepository;
		_unitOfWork = unitOfWork;
		_logger = logger;
	}

	public async Task<ServiceResponse> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Deleting product {ProductId} for user {UserId}", request.ProductId, request.UserId);

		try
		{
			var store = await _storeRepository.GetByUserIdAsync(request.UserId);
			if (store == null)
			{
				_logger.LogWarning("Store for user {UserId} not found", request.UserId);
				return new ServiceResponse(false, "Store not found");
			}

			var product = await _productRepository.GetByIdAsync(request.ProductId);
			if (product == null || product.StoreId != store.Id)
			{
				_logger.LogWarning("Product {ProductId} not found for store {StoreId}", request.ProductId, store.Id);
				return new ServiceResponse(false, "Product not found");
			}

			_productRepository.Delete(product);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			_logger.LogInformation("Product {ProductId} deleted successfully", product.Id);
			return new ServiceResponse(true, "Product deleted successfully");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting product {ProductId}", request.ProductId);
			return new ServiceResponse(false, $"Error: {ex.Message}");
		}
	}
}
