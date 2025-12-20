using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Product.Sku.AddSkuToProduct;

public sealed class AddSkuToProductCommandHandler : IRequestHandler<AddSkuToProductCommand, ServiceResponse<Guid>>
{
	private readonly IProductRepository _productRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<AddSkuToProductCommandHandler> _logger;

	public AddSkuToProductCommandHandler(
		IProductRepository productRepository,
		IUnitOfWork unitOfWork,
		ILogger<AddSkuToProductCommandHandler> logger)
	{
		_productRepository = productRepository;
		_unitOfWork = unitOfWork;
		_logger = logger;
	}

	public async Task<ServiceResponse<Guid>> Handle(AddSkuToProductCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Adding SKU to product {ProductId} by user {UserId}", request.ProductId, request.UserId);

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

			var sku = SkuEntity.Create(product.Id, request.Price, request.StockQuantity, request.Attributes);
			product.AddSku(sku);

			await _unitOfWork.SaveChangesAsync(cancellationToken);
			return new ServiceResponse<Guid>(true, "SKU added successfully", sku.Id);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding SKU to product {ProductId}", request.ProductId);
			return new ServiceResponse<Guid>(false, $"Error: {ex.Message}");
		}
	}
}
