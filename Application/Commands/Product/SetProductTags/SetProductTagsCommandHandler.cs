using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Product.SetProductTags;

public sealed class SetProductTagsCommandHandler : IRequestHandler<SetProductTagsCommand, ServiceResponse>
{
	private readonly IProductRepository _productRepository;
	private readonly ITagRepository _tagRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<SetProductTagsCommandHandler> _logger;

	public SetProductTagsCommandHandler(
		IProductRepository productRepository,
		ITagRepository tagRepository,
		IUnitOfWork unitOfWork,
		ILogger<SetProductTagsCommandHandler> logger)
	{
		_productRepository = productRepository;
		_tagRepository = tagRepository;
		_unitOfWork = unitOfWork;
		_logger = logger;
	}

	public async Task<ServiceResponse> Handle(SetProductTagsCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Setting product tags for product {ProductId} by user {UserId}", request.ProductId, request.UserId);

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

			var targetIds = (request.TagIds ?? new List<Guid>())
				.Where(x => x != Guid.Empty)
				.Distinct()
				.ToHashSet();

			// Remove tags not in target
			foreach (var existingId in product.ProductTags.Select(pt => pt.TagId).ToList())
			{
				if (!targetIds.Contains(existingId))
				{
					product.RemoveTag(existingId);
				}
			}

			// Add missing tags
			foreach (var tagId in targetIds)
			{
				if (product.ProductTags.Any(pt => pt.TagId == tagId))
				{
					continue;
				}

				var tag = await _tagRepository.GetByIdAsync(tagId);
				if (tag is null)
				{
					_logger.LogWarning("Tag {TagId} not found", tagId);
					return new ServiceResponse(false, "Tag not found");
				}

				product.AddTag(tag);
			}

			await _unitOfWork.SaveChangesAsync(cancellationToken);
			return new ServiceResponse(true, "Product tags updated successfully");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error setting product tags for product {ProductId}", request.ProductId);
			return new ServiceResponse(false, $"Error: {ex.Message}");
		}
	}
}
