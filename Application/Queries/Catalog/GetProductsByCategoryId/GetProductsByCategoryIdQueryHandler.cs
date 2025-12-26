using Application.DTOs;
using Application.Queries.Catalog;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Catalog.GetProductsByCategoryId;

public sealed class GetProductsByCategoryIdQueryHandler
	: IRequestHandler<GetProductsByCategoryIdQuery, ServiceResponse<IReadOnlyList<ProductSummaryDto>>>
{
	private readonly IProductRepository _productRepository;
	private readonly ILogger<GetProductsByCategoryIdQueryHandler> _logger;

	public GetProductsByCategoryIdQueryHandler(IProductRepository productRepository, ILogger<GetProductsByCategoryIdQueryHandler> logger)
	{
		_productRepository = productRepository;
		_logger = logger;
	}

	public async Task<ServiceResponse<IReadOnlyList<ProductSummaryDto>>> Handle(GetProductsByCategoryIdQuery request, CancellationToken cancellationToken)
	{
		try
		{
			if (request.CategoryId == Guid.Empty)
			{
				return new ServiceResponse<IReadOnlyList<ProductSummaryDto>>(false, "CategoryId is required");
			}

			var products = await _productRepository.GetByCategoryIdAsync(request.CategoryId);
			var payload = products
				.Select(ProductMapping.MapSummary)
				.ToList()
				.AsReadOnly();

			return new ServiceResponse<IReadOnlyList<ProductSummaryDto>>(true, "Products retrieved successfully", payload);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving products by category {CategoryId}", request.CategoryId);
			return new ServiceResponse<IReadOnlyList<ProductSummaryDto>>(false, $"Error: {ex.Message}");
		}
	}
}
