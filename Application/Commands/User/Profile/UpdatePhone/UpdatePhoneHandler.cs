using Application.DTOs;
using Application.ViewModels;
using MediatR;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Commands.User.Profile.UpdatePhone;

public class UpdatePhoneHandler : IRequestHandler<UpdatePhoneCommand, ServiceResponse<UserDto>>
{
	private readonly IUserService _identity;
	private readonly ILogger<UpdatePhoneHandler> _logger;

	public UpdatePhoneHandler(IUserService identity, ILogger<UpdatePhoneHandler> logger)
	{
		_identity = identity;
		_logger = logger;
	}

	public async Task<ServiceResponse<UserDto>> Handle(UpdatePhoneCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Updating phone number for user {UserId} to {NewPhoneNumber}", request.UserId, request.Data.PhoneNumber);

		try
		{
			var dto = await _identity.UpdatePhoneAsync(request.UserId, request.Data.PhoneNumber);
			if (dto == null)
			{
				_logger.LogWarning("Phone update failed for user {UserId}", request.UserId);
				return new ServiceResponse<UserDto>(false, "Phone update failed");
			}

			_logger.LogInformation("Successfully updated phone number for user {UserId}", request.UserId);
			return new ServiceResponse<UserDto>(true, "Phone updated", dto);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating phone number for user {UserId}", request.UserId);
			return new ServiceResponse<UserDto>(false, $"Error: {ex.Message}");
		}
	}
}
