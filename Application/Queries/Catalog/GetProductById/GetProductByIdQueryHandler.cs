using Application.DTOs;
using Application.Interfaces;
using Application.Queries.Catalog;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Catalog.GetProductById;

public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ServiceResponse<ProductDetailsDto>>
{
	private readonly IProductRepository _productRepository;
	private readonly IFileStorage _fileStorage;
	private readonly ILogger<GetProductByIdQueryHandler> _logger;

	public GetProductByIdQueryHandler(IProductRepository productRepository, IFileStorage fileStorage, ILogger<GetProductByIdQueryHandler> logger)
	{
		_productRepository = productRepository;
		_fileStorage = fileStorage;
		_logger = logger;
	}

	public async Task<ServiceResponse<ProductDetailsDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
	{
		try
		{
			var product = await _productRepository.GetByIdAsync(request.Id);
			if (product is null)
			{
				return new ServiceResponse<ProductDetailsDto>(false, "Product not found");
			}

			return new ServiceResponse<ProductDetailsDto>(true, "Product retrieved successfully", ProductMapping.MapDetails(product, _fileStorage));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving product {ProductId}", request.Id);
			return new ServiceResponse<ProductDetailsDto>(false, $"Error: {ex.Message}");
		}
	}
}
