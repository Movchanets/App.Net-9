using Application.DTOs;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Store.GetAllStores;

public sealed class GetAllStoresQueryHandler : IRequestHandler<GetAllStoresQuery, ServiceResponse<IReadOnlyList<StoreAdminDto>>>
{
	private readonly IStoreRepository _storeRepository;
	private readonly ILogger<GetAllStoresQueryHandler> _logger;

	public GetAllStoresQueryHandler(IStoreRepository storeRepository, ILogger<GetAllStoresQueryHandler> logger)
	{
		_storeRepository = storeRepository;
		_logger = logger;
	}

	public async Task<ServiceResponse<IReadOnlyList<StoreAdminDto>>> Handle(GetAllStoresQuery request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Getting all stores, includeUnverified: {IncludeUnverified}", request.IncludeUnverified);

		try
		{
			var stores = await _storeRepository.GetAllAsync(request.IncludeUnverified);

			var dtos = stores.Select(s => new StoreAdminDto(
				s.Id,
				s.Name,
				s.Slug,
				s.Description,
				s.UserId,
				s.User?.Name,
				s.User?.Email,
				s.IsVerified,
				s.IsSuspended,
				s.CreatedAt,
				s.Products.Count
			)).ToList();

			_logger.LogInformation("Retrieved {Count} stores", dtos.Count);
			return new ServiceResponse<IReadOnlyList<StoreAdminDto>>(true, "Stores retrieved successfully", dtos);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting all stores");
			return new ServiceResponse<IReadOnlyList<StoreAdminDto>>(false, $"Error: {ex.Message}");
		}
	}
}
