using Application.ViewModels;
using Infrastructure.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Commands.User.Profile.ChangePassword;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, ServiceResponse>
{
	private readonly UserManager<UserEntity> _userManager;

	public ChangePasswordHandler(UserManager<UserEntity> userManager)
	{
		_userManager = userManager;
	}

	public async Task<ServiceResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
	{
		var user = await _userManager.FindByIdAsync(request.UserId.ToString());
		if (user == null) return new ServiceResponse(false, "User not found");

		var result = await _userManager.ChangePasswordAsync(user, request.Data.CurrentPassword, request.Data.NewPassword);
		if (!result.Succeeded) return new ServiceResponse(false, "Failed to change password");

		return new ServiceResponse(true, "Password changed");
	}
}
