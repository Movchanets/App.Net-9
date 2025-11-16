using Application.DTOs;
using Application.ViewModels;
using MediatR;
using Application.Interfaces;

namespace Application.Commands.User.Profile.UpdateEmail;

public class UpdateEmailHandler : IRequestHandler<UpdateEmailCommand, ServiceResponse<UserDto>>
{
	private readonly IUserService _identity;

	public UpdateEmailHandler(IUserService identity)
	{
		_identity = identity;
	}

	public async Task<ServiceResponse<UserDto>> Handle(UpdateEmailCommand request, CancellationToken cancellationToken)
	{
		var dto = await _identity.UpdateEmailAsync(request.UserId, request.Data.Email);
		if (dto == null) return new ServiceResponse<UserDto>(false, "Email update failed");
		return new ServiceResponse<UserDto>(true, "Email updated", dto);
	}
}
