using Application.DTOs;
using Application.Interfaces;
using Application.ViewModels;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.User.Profile.DeleteProfilePicture;

public class DeleteProfilePictureHandler : IRequestHandler<DeleteProfilePictureCommand, ServiceResponse<UserDto>>
{
	private readonly IUserService _userService;
	private readonly ILogger<DeleteProfilePictureHandler> _logger;

	public DeleteProfilePictureHandler(IUserService userService, ILogger<DeleteProfilePictureHandler> logger)
	{
		_userService = userService;
		_logger = logger;
	}

	public async Task<ServiceResponse<UserDto>> Handle(DeleteProfilePictureCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Deleting profile picture for user {UserId}", request.UserId);

		try
		{
			var dto = await _userService.DeleteProfilePictureAsync(request.UserId, cancellationToken);

			if (dto == null)
			{
				_logger.LogWarning("Profile picture deletion failed for user {UserId}", request.UserId);
				return new ServiceResponse<UserDto>(false, "Profile picture deletion failed");
			}

			_logger.LogInformation("Successfully deleted profile picture for user {UserId}", request.UserId);
			return new ServiceResponse<UserDto>(true, "Profile picture deleted successfully", dto);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting profile picture for user {UserId}", request.UserId);
			return new ServiceResponse<UserDto>(false, $"Error: {ex.Message}");
		}
	}
}
