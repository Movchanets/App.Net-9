using Application.DTOs;
using MediatR;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Commands.User.Profile.UpdateProfile;

public class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, ServiceResponse<UserDto>>
{
	private readonly IUserService _identity;
	private readonly ILogger<UpdateProfileHandler> _logger;

	public UpdateProfileHandler(IUserService identity, ILogger<UpdateProfileHandler> logger)
	{
		_identity = identity;
		_logger = logger;
	}

	public async Task<ServiceResponse<UserDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Updating identity profile for user {UserId}: username={Username}, name={Name}, surname={Surname}",
			request.UserId, request.Data.Username, request.Data.Name, request.Data.Surname);

		try
		{
			var dto = await _identity.UpdateIdentityProfileAsync(request.UserId, request.Data.Username, request.Data.Name, request.Data.Surname);
			if (dto == null)
			{
				_logger.LogWarning("Identity profile update failed for user {UserId}", request.UserId);
				return new ServiceResponse<UserDto>(false, "Profile update failed");
			}

			_logger.LogInformation("Successfully updated identity profile for user {UserId}", request.UserId);
			return new ServiceResponse<UserDto>(true, "Profile updated", dto);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating identity profile for user {UserId}", request.UserId);
			return new ServiceResponse<UserDto>(false, $"Error: {ex.Message}");
		}
	}
}
