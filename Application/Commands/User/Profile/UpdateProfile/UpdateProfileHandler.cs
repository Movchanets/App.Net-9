using Application.DTOs;
using Application.ViewModels;
using MediatR;
using Application.Interfaces;

namespace Application.Commands.User.Profile.UpdateProfile;

public class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, ServiceResponse<UserDto>>
{
	private readonly IUserService _identity;

	public UpdateProfileHandler(IUserService identity)
	{
		_identity = identity;
	}

	public async Task<ServiceResponse<UserDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
	{
		var dto = await _identity.UpdateIdentityProfileAsync(request.UserId, request.Data.Username, null, request.Data.PhoneNumber, request.Data.Name, request.Data.Surname);
		if (dto == null) return new ServiceResponse<UserDto>(false, "Profile update failed");
		return new ServiceResponse<UserDto>(true, "Profile updated", dto);
	}
}
