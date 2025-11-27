using Application.DTOs;
using MediatR;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Commands.User.Profile.UpdateProfileInfo;

public class UpdateProfileInfoHandler : IRequestHandler<UpdateProfileInfoCommand, ServiceResponse<UserDto>>
{
	private readonly IUserService _identity;
	private readonly ILogger<UpdateProfileInfoHandler> _logger;

	public UpdateProfileInfoHandler(IUserService identity, ILogger<UpdateProfileInfoHandler> logger)
	{
		_identity = identity;
		_logger = logger;
	}

	public async Task<ServiceResponse<UserDto>> Handle(UpdateProfileInfoCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Updating profile info for user {UserId}: username={Username}, name={Name}, surname={Surname}",
			request.UserId, request.Data.Username, request.Data.Name, request.Data.Surname);

		try
		{
			var dto = await _identity.UpdateProfileInfoAsync(request.UserId, request.Data.Username, request.Data.Name, request.Data.Surname);
			if (dto == null)
			{
				_logger.LogWarning("Profile info update failed for user {UserId}", request.UserId);
				return new ServiceResponse<UserDto>(false, "Profile info update failed");
			}

			_logger.LogInformation("Successfully updated profile info for user {UserId}", request.UserId);
			return new ServiceResponse<UserDto>(true, "Profile info updated", dto);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating profile info for user {UserId}", request.UserId);
			return new ServiceResponse<UserDto>(false, $"Error: {ex.Message}");
		}
	}
}
