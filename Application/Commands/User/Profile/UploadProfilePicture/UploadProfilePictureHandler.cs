using Application.DTOs;
using Application.Interfaces;
using Application.ViewModels;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.User.Profile.UploadProfilePicture;

public class UploadProfilePictureHandler : IRequestHandler<UploadProfilePictureCommand, ServiceResponse<UserDto>>
{
	private readonly IUserService _userService;
	private readonly ILogger<UploadProfilePictureHandler> _logger;

	public UploadProfilePictureHandler(IUserService userService, ILogger<UploadProfilePictureHandler> logger)
	{
		_userService = userService;
		_logger = logger;
	}

	public async Task<ServiceResponse<UserDto>> Handle(UploadProfilePictureCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Uploading profile picture for user {UserId}, fileName: {FileName}", request.UserId, request.FileName);

		try
		{
			var dto = await _userService.UpdateProfilePictureAsync(
				request.UserId,
				request.FileStream,
				request.FileName,
				request.ContentType,
				cancellationToken
			);

			if (dto == null)
			{
				_logger.LogWarning("Profile picture upload failed for user {UserId}", request.UserId);
				return new ServiceResponse<UserDto>(false, "Profile picture upload failed");
			}

			_logger.LogInformation("Successfully uploaded profile picture for user {UserId}", request.UserId);
			return new ServiceResponse<UserDto>(true, "Profile picture uploaded successfully", dto);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error uploading profile picture for user {UserId}", request.UserId);
			return new ServiceResponse<UserDto>(false, $"Error: {ex.Message}");
		}
	}
}
