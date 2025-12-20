using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Product.Sku.DeleteSku;

public sealed class DeleteSkuCommandHandler : IRequestHandler<DeleteSkuCommand, ServiceResponse>
{
	private readonly ISkuRepository _skuRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<DeleteSkuCommandHandler> _logger;

	public DeleteSkuCommandHandler(
		ISkuRepository skuRepository,
		IUnitOfWork unitOfWork,
		ILogger<DeleteSkuCommandHandler> logger)
	{
		_skuRepository = skuRepository;
		_unitOfWork = unitOfWork;
		_logger = logger;
	}

	public async Task<ServiceResponse> Handle(DeleteSkuCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Deleting SKU {SkuId} from product {ProductId} by user {UserId}", request.SkuId, request.ProductId, request.UserId);

		try
		{
			var sku = await _skuRepository.GetByIdAsync(request.SkuId);
			if (sku?.Product is null || sku.ProductId != request.ProductId || sku.Product.Store is null || sku.Product.Store.UserId != request.UserId)
			{
				_logger.LogWarning("SKU {SkuId} not found for product {ProductId} and user {UserId}", request.SkuId, request.ProductId, request.UserId);
				return new ServiceResponse(false, "SKU not found");
			}

			if (sku.Product.Store.IsSuspended)
			{
				_logger.LogWarning("Store {StoreId} is suspended", sku.Product.Store.Id);
				return new ServiceResponse(false, "Store is suspended");
			}

			_skuRepository.Delete(sku);
			await _unitOfWork.SaveChangesAsync(cancellationToken);
			return new ServiceResponse(true, "SKU deleted successfully");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting SKU {SkuId} from product {ProductId}", request.SkuId, request.ProductId);
			return new ServiceResponse(false, $"Error: {ex.Message}");
		}
	}
}
