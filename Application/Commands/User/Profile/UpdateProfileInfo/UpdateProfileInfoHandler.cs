using Application.DTOs;
using Application.ViewModels;
using MediatR;
using Application.Interfaces;

namespace Application.Commands.User.Profile.UpdateProfileInfo;

public class UpdateProfileInfoHandler : IRequestHandler<UpdateProfileInfoCommand, ServiceResponse<UserDto>>
{
	private readonly IUserService _identity;

	public UpdateProfileInfoHandler(IUserService identity)
	{
		_identity = identity;
	}

	public async Task<ServiceResponse<UserDto>> Handle(UpdateProfileInfoCommand request, CancellationToken cancellationToken)
	{
		var dto = await _identity.UpdateProfileInfoAsync(request.UserId, request.Data.Username, request.Data.Name, request.Data.Surname);
		if (dto == null) return new ServiceResponse<UserDto>(false, "Profile info update failed");
		return new ServiceResponse<UserDto>(true, "Profile info updated", dto);
	}
}
