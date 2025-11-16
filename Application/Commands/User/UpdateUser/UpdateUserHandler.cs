using Application.DTOs;
using Application.ViewModels;
using MediatR;
using Application.Interfaces;

namespace Application.Commands.User.UpdateUser;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, ServiceResponse<UserDto>>
{
	private readonly IUserService _identity;

	public UpdateUserHandler(IUserService identity)
	{
		_identity = identity;
	}

	public async Task<ServiceResponse<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
	{
		var dto = await _identity.UpdateIdentityProfileAsync(request.Id, null, request.Data.Email, request.Data.PhoneNumber, request.Data.Name, request.Data.Surname);
		if (dto == null)
			return new ServiceResponse<UserDto>(false, "Failed to update user");
		return new ServiceResponse<UserDto>(true, "User updated", dto);
	}
}
