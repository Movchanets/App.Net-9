using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Product.SetProductCategories;

public sealed class SetProductCategoriesCommandHandler : IRequestHandler<SetProductCategoriesCommand, ServiceResponse>
{
	private readonly IProductRepository _productRepository;
	private readonly ICategoryRepository _categoryRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<SetProductCategoriesCommandHandler> _logger;

	public SetProductCategoriesCommandHandler(
		IProductRepository productRepository,
		ICategoryRepository categoryRepository,
		IUnitOfWork unitOfWork,
		ILogger<SetProductCategoriesCommandHandler> logger)
	{
		_productRepository = productRepository;
		_categoryRepository = categoryRepository;
		_unitOfWork = unitOfWork;
		_logger = logger;
	}

	public async Task<ServiceResponse> Handle(SetProductCategoriesCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Setting product categories for product {ProductId} by user {UserId}", request.ProductId, request.UserId);

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

			var targetIds = (request.CategoryIds ?? new List<Guid>())
				.Where(x => x != Guid.Empty)
				.Distinct()
				.ToHashSet();

			// Remove categories not in target
			foreach (var existingId in product.ProductCategories.Select(pc => pc.CategoryId).ToList())
			{
				if (!targetIds.Contains(existingId))
				{
					product.RemoveCategory(existingId);
				}
			}

			// Add missing categories
			foreach (var categoryId in targetIds)
			{
				if (product.ProductCategories.Any(pc => pc.CategoryId == categoryId))
				{
					continue;
				}

				var category = await _categoryRepository.GetByIdAsync(categoryId);
				if (category is null)
				{
					_logger.LogWarning("Category {CategoryId} not found", categoryId);
					return new ServiceResponse(false, "Category not found");
				}

				product.AddCategory(category);
			}

			await _unitOfWork.SaveChangesAsync(cancellationToken);
			return new ServiceResponse(true, "Product categories updated successfully");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error setting product categories for product {ProductId}", request.ProductId);
			return new ServiceResponse(false, $"Error: {ex.Message}");
		}
	}
}
