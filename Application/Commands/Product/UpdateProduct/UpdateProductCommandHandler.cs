using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Product.UpdateProduct;

public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ServiceResponse>
{
	private readonly IProductRepository _productRepository;
	private readonly IStoreRepository _storeRepository;
	private readonly ICategoryRepository _categoryRepository;
	private readonly ITagRepository _tagRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<UpdateProductCommandHandler> _logger;

	public UpdateProductCommandHandler(
		IProductRepository productRepository,
		IStoreRepository storeRepository,
		ICategoryRepository categoryRepository,
		ITagRepository tagRepository,
		IUnitOfWork unitOfWork,
		ILogger<UpdateProductCommandHandler> logger)
	{
		_productRepository = productRepository;
		_storeRepository = storeRepository;
		_categoryRepository = categoryRepository;
		_tagRepository = tagRepository;
		_unitOfWork = unitOfWork;
		_logger = logger;
	}

	public async Task<ServiceResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Updating product {ProductId} for user {UserId}", request.ProductId, request.UserId);

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

			var categoryIds = (request.CategoryIds ?? new List<Guid>())
				.Where(x => x != Guid.Empty)
				.Distinct()
				.ToList();

			// Replace categories
			foreach (var categoryId in product.ProductCategories.Select(pc => pc.CategoryId).ToList())
			{
				product.RemoveCategory(categoryId);
			}

			foreach (var categoryId in categoryIds)
			{
				var category = await _categoryRepository.GetByIdAsync(categoryId);
				if (category == null)
				{
					_logger.LogWarning("Category {CategoryId} not found", categoryId);
					return new ServiceResponse(false, "Category not found");
				}

				product.AddCategory(category);
			}

			// Replace tags
			foreach (var tagId in product.ProductTags.Select(pt => pt.TagId).ToList())
			{
				product.RemoveTag(tagId);
			}

			if (request.TagIds is { Count: > 0 })
			{
				foreach (var tagId in request.TagIds.Distinct())
				{
					var tag = await _tagRepository.GetByIdAsync(tagId);
					if (tag == null)
					{
						_logger.LogWarning("Tag {TagId} not found", tagId);
						return new ServiceResponse(false, "Tag not found");
					}

					product.AddTag(tag);
				}
			}

			product.Rename(request.Name);
			product.UpdateDescription(request.Description);

			await _unitOfWork.SaveChangesAsync(cancellationToken);

			_logger.LogInformation("Product {ProductId} updated successfully", product.Id);
			return new ServiceResponse(true, "Product updated successfully");
		}
		catch (DbUpdateConcurrencyException ex)
		{
			var entryInfo = ex.Entries.Count == 0
				? "<none>"
				: string.Join(", ", ex.Entries.Select(e =>
				{
					var key = e.Properties
						.Where(p => p.Metadata.IsPrimaryKey())
						.Select(p => $"{p.Metadata.Name}={p.CurrentValue}")
						.DefaultIfEmpty("<no-pk>")
						.First();

					return $"{e.Metadata.ClrType.Name}({key})";
				}));

			_logger.LogError(ex, "Optimistic concurrency error updating product {ProductId}. Entries: {Entries}", request.ProductId, entryInfo);
			return new ServiceResponse(false, $"Error: {ex.Message}. Entries: {entryInfo}");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating product {ProductId}", request.ProductId);
			return new ServiceResponse(false, $"Error: {ex.Message}");
		}
	}
}
