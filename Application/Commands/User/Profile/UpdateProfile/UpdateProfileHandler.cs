using Application.DTOs;
using Application.ViewModels;
using Infrastructure.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Commands.User.Profile.UpdateProfile;

public class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, ServiceResponse<UserDto>>
{
	private readonly UserManager<UserEntity> _userManager;

	public UpdateProfileHandler(UserManager<UserEntity> userManager)
	{
		_userManager = userManager;
	}

	public async Task<ServiceResponse<UserDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
	{
		var user = await _userManager.FindByIdAsync(request.UserId.ToString());
		if (user == null) return new ServiceResponse<UserDto>(false, "User not found");

		// Update allowed profile fields
		if (!string.IsNullOrWhiteSpace(request.Data.Name)) user.Name = request.Data.Name;
		if (!string.IsNullOrWhiteSpace(request.Data.Surname)) user.Surname = request.Data.Surname;
		if (!string.IsNullOrWhiteSpace(request.Data.Username))
		{
			// If username changed, ensure uniqueness
			var existing = await _userManager.FindByNameAsync(request.Data.Username);
			if (existing != null && existing.Id != user.Id)
			{
				return new ServiceResponse<UserDto>(false, "Username is already taken");
			}
			user.UserName = request.Data.Username;
		}
		if (!string.IsNullOrWhiteSpace(request.Data.PhoneNumber)) user.PhoneNumber = request.Data.PhoneNumber;

		var result = await _userManager.UpdateAsync(user);
		if (!result.Succeeded) return new ServiceResponse<UserDto>(false, "Failed to update profile");

		var roles = (await _userManager.GetRolesAsync(user)).ToList();
		var dto = new UserDto(user.UserName ?? string.Empty, user.Name ?? string.Empty, user.Surname ?? string.Empty, user.Email ?? string.Empty, user.PhoneNumber ?? string.Empty, roles);

		return new ServiceResponse<UserDto>(true, "Profile updated", dto);
	}
}
