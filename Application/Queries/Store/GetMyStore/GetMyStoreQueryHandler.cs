using Application.DTOs;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Store.GetMyStore;

public sealed class GetMyStoreQueryHandler : IRequestHandler<GetMyStoreQuery, ServiceResponse<MyStoreDto?>>
{
	private readonly IStoreRepository _storeRepository;
	private readonly IUserRepository _userRepository;
	private readonly ILogger<GetMyStoreQueryHandler> _logger;

	public GetMyStoreQueryHandler(
		IStoreRepository storeRepository,
		IUserRepository userRepository,
		ILogger<GetMyStoreQueryHandler> logger)
	{
		_storeRepository = storeRepository;
		_userRepository = userRepository;
		_logger = logger;
	}

	public async Task<ServiceResponse<MyStoreDto?>> Handle(GetMyStoreQuery request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Getting store for user {UserId}", request.UserId);

		try
		{
			// UserId from JWT is IdentityUserId, need to lookup DomainUser
			var domainUser = await _userRepository.GetByIdentityUserIdAsync(request.UserId);
			if (domainUser == null)
			{
				_logger.LogWarning("Domain user not found for identity user {UserId}", request.UserId);
				return new ServiceResponse<MyStoreDto?>(false, "User not found");
			}

			var store = await _storeRepository.GetByUserIdAsync(domainUser.Id);

			if (store is null)
			{
				_logger.LogInformation("No store found for user {UserId}", domainUser.Id);
				return new ServiceResponse<MyStoreDto?>(true, "No store found", null);
			}

			var dto = new MyStoreDto(
				store.Id,
				store.Name,
				store.Slug,
				store.Description,
				store.IsVerified,
				store.IsSuspended,
				store.CreatedAt,
				store.Products.Count
			);

			_logger.LogInformation("Store {StoreId} found for user {UserId}", store.Id, domainUser.Id);
			return new ServiceResponse<MyStoreDto?>(true, "Store retrieved successfully", dto);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting store for user {UserId}", request.UserId);
			return new ServiceResponse<MyStoreDto?>(false, $"Error: {ex.Message}");
		}
	}
}
