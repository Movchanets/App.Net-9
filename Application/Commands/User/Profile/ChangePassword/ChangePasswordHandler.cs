using Application.ViewModels;
using MediatR;
using Application.Interfaces;

namespace Application.Commands.User.Profile.ChangePassword;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, ServiceResponse>
{
	private readonly IUserService _identity;

	public ChangePasswordHandler(IUserService identity)
	{
		_identity = identity;
	}

	public async Task<ServiceResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
	{
		var ok = await _identity.ChangePasswordAsync(request.UserId, request.Data.CurrentPassword, request.Data.NewPassword);
		if (!ok) return new ServiceResponse(false, "Failed to change password");

		return new ServiceResponse(true, "Password changed");
	}
}
