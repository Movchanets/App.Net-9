using Application.DTOs;
using Application.ViewModels;
using Infrastructure.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Commands.User.UpdateUser;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, ServiceResponse<UserDto>>
{
	private readonly UserManager<UserEntity> _userManager;

	public UpdateUserHandler(UserManager<UserEntity> userManager)
	{
		_userManager = userManager;
	}

	public async Task<ServiceResponse<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
	{
		var user = await _userManager.FindByIdAsync(request.Id.ToString());
		if (user == null)
			return new ServiceResponse<UserDto>(false, "User not found");

		// Update allowed fields
		if (!string.IsNullOrWhiteSpace(request.Data.Name)) user.Name = request.Data.Name;
		if (!string.IsNullOrWhiteSpace(request.Data.Surname)) user.Surname = request.Data.Surname;
		if (!string.IsNullOrWhiteSpace(request.Data.Email)) user.Email = request.Data.Email;
		if (!string.IsNullOrWhiteSpace(request.Data.PhoneNumber)) user.PhoneNumber = request.Data.PhoneNumber;

		var result = await _userManager.UpdateAsync(user);
		if (!result.Succeeded)
			return new ServiceResponse<UserDto>(false, "Failed to update user");

		var roles = (await _userManager.GetRolesAsync(user)).ToList();

		var dto = new UserDto(user.UserName ?? string.Empty, user.Name ?? string.Empty, user.Surname ?? string.Empty, user.Email ?? string.Empty, user.PhoneNumber ?? string.Empty, roles);

		return new ServiceResponse<UserDto>(true, "User updated", dto);
	}
}
