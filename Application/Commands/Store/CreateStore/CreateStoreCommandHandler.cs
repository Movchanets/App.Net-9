using Application.DTOs;
using Application.Interfaces;
using Domain.Helpers;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using StoreEntity = Domain.Entities.Store;

namespace Application.Commands.Store.CreateStore;

public sealed class CreateStoreCommandHandler : IRequestHandler<CreateStoreCommand, ServiceResponse<Guid>>
{
	private readonly IStoreRepository _storeRepository;
	private readonly IUserRepository _userRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<CreateStoreCommandHandler> _logger;

	public CreateStoreCommandHandler(
		IStoreRepository storeRepository,
		IUserRepository userRepository,
		IUnitOfWork unitOfWork,
		ILogger<CreateStoreCommandHandler> logger)
	{
		_storeRepository = storeRepository;
		_userRepository = userRepository;
		_unitOfWork = unitOfWork;
		_logger = logger;
	}

	public async Task<ServiceResponse<Guid>> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Creating store {StoreName} for user {UserId}", request.Name, request.UserId);

		try
		{
			if (request.UserId == Guid.Empty)
			{
				return new ServiceResponse<Guid>(false, "UserId is required");
			}

			var user = await _userRepository.GetByIdAsync(request.UserId);
			if (user == null)
			{
				_logger.LogWarning("User {UserId} not found", request.UserId);
				return new ServiceResponse<Guid>(false, "User not found");
			}

			var existingStore = await _storeRepository.GetByUserIdAsync(request.UserId);
			if (existingStore != null)
			{
				_logger.LogWarning("User {UserId} already has a store {StoreId}", request.UserId, existingStore.Id);
				return new ServiceResponse<Guid>(false, "User already has a store");
			}

			var slug = SlugHelper.GenerateSlug(request.Name);
			var existingBySlug = await _storeRepository.GetBySlugAsync(slug);
			if (existingBySlug != null)
			{
				_logger.LogWarning("Store slug already exists {Slug}", slug);
				return new ServiceResponse<Guid>(false, "Store with same slug already exists");
			}

			var store = StoreEntity.Create(request.UserId, request.Name, request.Description);

			_storeRepository.Add(store);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			_logger.LogInformation("Store {StoreId} created for user {UserId}", store.Id, request.UserId);
			return new ServiceResponse<Guid>(true, "Store created successfully", store.Id);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating store for user {UserId}", request.UserId);
			return new ServiceResponse<Guid>(false, $"Error: {ex.Message}");
		}
	}
}
