using Application.DTOs;
using Application.ViewModels;
using Infrastructure.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Commands.User.Profile.GetProfile;

public class GetProfileHandler : IRequestHandler<GetProfileQuery, ServiceResponse<UserDto>>
{
	private readonly UserManager<UserEntity> _userManager;

	public GetProfileHandler(UserManager<UserEntity> userManager)
	{
		_userManager = userManager;
	}

	public async Task<ServiceResponse<UserDto>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
	{
		var user = await _userManager.FindByIdAsync(request.UserId.ToString());
		if (user == null) return new ServiceResponse<UserDto>(false, "User not found");

		var roles = (await _userManager.GetRolesAsync(user)).ToList();

		var dto = new UserDto(user.UserName ?? string.Empty, user.Name ?? string.Empty, user.Surname ?? string.Empty, user.Email ?? string.Empty, user.PhoneNumber ?? string.Empty, roles);

		return new ServiceResponse<UserDto>(true, "Profile retrieved", dto);
	}
}
