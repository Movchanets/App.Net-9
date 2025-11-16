using Application.DTOs;
using Application.ViewModels;
using MediatR;
using Application.Interfaces;

namespace Application.Commands.User.Profile.UpdatePhone;

public class UpdatePhoneHandler : IRequestHandler<UpdatePhoneCommand, ServiceResponse<UserDto>>
{
	private readonly IUserService _identity;

	public UpdatePhoneHandler(IUserService identity)
	{
		_identity = identity;
	}

	public async Task<ServiceResponse<UserDto>> Handle(UpdatePhoneCommand request, CancellationToken cancellationToken)
	{
		var dto = await _identity.UpdatePhoneAsync(request.UserId, request.Data.PhoneNumber);
		if (dto == null) return new ServiceResponse<UserDto>(false, "Phone update failed");
		return new ServiceResponse<UserDto>(true, "Phone updated", dto);
	}
}
