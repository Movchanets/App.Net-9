using Application.ViewModels;
using MediatR;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Commands.User.Profile.ChangePassword;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, ServiceResponse>
{
	private readonly IUserService _identity;
	private readonly ILogger<ChangePasswordHandler> _logger;

	public ChangePasswordHandler(IUserService identity, ILogger<ChangePasswordHandler> logger)
	{
		_identity = identity;
		_logger = logger;
	}

	public async Task<ServiceResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Changing password for user {UserId}", request.UserId);

		try
		{
			var ok = await _identity.ChangePasswordAsync(request.UserId, request.Data.CurrentPassword, request.Data.NewPassword);
			if (!ok)
			{
				_logger.LogWarning("Password change failed for user {UserId}", request.UserId);
				return new ServiceResponse(false, "Failed to change password");
			}

			_logger.LogInformation("Successfully changed password for user {UserId}", request.UserId);
			return new ServiceResponse(true, "Password changed");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error changing password for user {UserId}", request.UserId);
			return new ServiceResponse(false, $"Error: {ex.Message}");
		}
	}
}
