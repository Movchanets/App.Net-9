using Infrastructure.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Application.ViewModels;

namespace Application.Commands.User.DeleteUser;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, ServiceResponse>
{
	private readonly UserManager<UserEntity> _userManager;

	public DeleteUserHandler(UserManager<UserEntity> userManager)
	{
		_userManager = userManager;
	}

	public async Task<ServiceResponse> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
	{
		var user = await _userManager.FindByIdAsync(request.Id.ToString());
		if (user == null)
			return new ServiceResponse(false, "User not found");

		var result = await _userManager.DeleteAsync(user);
		if (!result.Succeeded)
			return new ServiceResponse(false, "Failed to delete user");

		return new ServiceResponse(true, "User deleted");
	}
}
