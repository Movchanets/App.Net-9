using Application.DTOs;
using Application.Interfaces;
using Application.Queries.Catalog;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Catalog.GetProductBySkuCode;

public sealed class GetProductBySkuCodeQueryHandler : IRequestHandler<GetProductBySkuCodeQuery, ServiceResponse<ProductDetailsDto>>
{
	private readonly IProductRepository _productRepository;
	private readonly IFileStorage _fileStorage;
	private readonly ILogger<GetProductBySkuCodeQueryHandler> _logger;

	public GetProductBySkuCodeQueryHandler(IProductRepository productRepository, IFileStorage fileStorage, ILogger<GetProductBySkuCodeQueryHandler> logger)
	{
		_productRepository = productRepository;
		_fileStorage = fileStorage;
		_logger = logger;
	}

	public async Task<ServiceResponse<ProductDetailsDto>> Handle(GetProductBySkuCodeQuery request, CancellationToken cancellationToken)
	{
		try
		{
			var skuCode = request.SkuCode?.Trim();
			if (string.IsNullOrWhiteSpace(skuCode))
			{
				return new ServiceResponse<ProductDetailsDto>(false, "SkuCode is required");
			}

			var product = await _productRepository.GetBySkuCodeAsync(skuCode);
			if (product is null)
			{
				return new ServiceResponse<ProductDetailsDto>(false, "Product not found");
			}

			return new ServiceResponse<ProductDetailsDto>(true, "Product retrieved successfully", ProductMapping.MapDetails(product, _fileStorage));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving product by sku {SkuCode}", request.SkuCode);
			return new ServiceResponse<ProductDetailsDto>(false, $"Error: {ex.Message}");
		}
	}
}
