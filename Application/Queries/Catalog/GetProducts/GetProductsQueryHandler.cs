using Application.DTOs;
using Application.Queries.Catalog;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Catalog.GetProducts;

public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, ServiceResponse<IReadOnlyList<ProductSummaryDto>>>
{
	private readonly IProductRepository _productRepository;
	private readonly ILogger<GetProductsQueryHandler> _logger;

	public GetProductsQueryHandler(IProductRepository productRepository, ILogger<GetProductsQueryHandler> logger)
	{
		_productRepository = productRepository;
		_logger = logger;
	}

	public async Task<ServiceResponse<IReadOnlyList<ProductSummaryDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
	{
		try
		{
			var products = await _productRepository.GetAllAsync();
			var payload = products
				.Select(ProductMapping.MapSummary)
				.ToList()
				.AsReadOnly();

			return new ServiceResponse<IReadOnlyList<ProductSummaryDto>>(true, "Products retrieved successfully", payload);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving products");
			return new ServiceResponse<IReadOnlyList<ProductSummaryDto>>(false, $"Error: {ex.Message}");
		}
	}
}
