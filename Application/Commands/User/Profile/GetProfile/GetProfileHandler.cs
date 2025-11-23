using Application.DTOs;
using Application.ViewModels;
using MediatR;
using Application.Interfaces;
using Domain.Interfaces.Repositories;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Application.Commands.User.Profile.GetProfile;

public class GetProfileHandler : IRequestHandler<GetProfileQuery, ServiceResponse<UserDto>>
{
	private readonly IUserService _identity;
	private readonly IUserRepository _userRepository;
	private readonly IMapper _mapper;
	private readonly ILogger<GetProfileHandler> _logger;

	public GetProfileHandler(IUserService identity, IUserRepository userRepository, IMapper mapper, ILogger<GetProfileHandler> logger)
	{
		_identity = identity;
		_userRepository = userRepository;
		_mapper = mapper;
		_logger = logger;
	}

	public async Task<ServiceResponse<UserDto>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Retrieving profile for user {UserId}", request.UserId);

		try
		{
			var dto = await _identity.GetIdentityInfoByIdAsync(request.UserId);
			if (dto == null)
			{
				_logger.LogWarning("Profile not found for user {UserId}", request.UserId);
				return new ServiceResponse<UserDto>(false, "User not found");
			}

			_logger.LogInformation("Successfully retrieved profile for user {UserId}", request.UserId);
			return new ServiceResponse<UserDto>(true, "Profile retrieved", dto);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving profile for user {UserId}", request.UserId);
			return new ServiceResponse<UserDto>(false, $"Error: {ex.Message}");
		}
	}
}
