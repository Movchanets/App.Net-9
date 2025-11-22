using Application.DTOs;
using Application.ViewModels;
using MediatR;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Commands.User.Profile.UpdateEmail;

public class UpdateEmailHandler : IRequestHandler<UpdateEmailCommand, ServiceResponse<UserDto>>
{
	private readonly IUserService _identity;
	private readonly ILogger<UpdateEmailHandler> _logger;

	public UpdateEmailHandler(IUserService identity, ILogger<UpdateEmailHandler> logger)
	{
		_identity = identity;
		_logger = logger;
	}

	public async Task<ServiceResponse<UserDto>> Handle(UpdateEmailCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Updating email for user {UserId} to {NewEmail}", request.UserId, request.Data.Email);

		try
		{
			var dto = await _identity.UpdateEmailAsync(request.UserId, request.Data.Email);
			if (dto == null)
			{
				_logger.LogWarning("Email update failed for user {UserId}", request.UserId);
				return new ServiceResponse<UserDto>(false, "Email update failed");
			}

			_logger.LogInformation("Successfully updated email for user {UserId}", request.UserId);
			return new ServiceResponse<UserDto>(true, "Email updated", dto);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating email for user {UserId}", request.UserId);
			return new ServiceResponse<UserDto>(false, $"Error: {ex.Message}");
		}
	}
}
