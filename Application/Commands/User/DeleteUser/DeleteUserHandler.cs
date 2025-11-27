using Application.DTOs;
using Application.Interfaces;
using MediatR;

namespace Application.Commands.User.DeleteUser;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, ServiceResponse>
{
	private readonly IUserService _identity;

	public DeleteUserHandler(IUserService identity)
	{
		_identity = identity;
	}

	public async Task<ServiceResponse> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
	{
		var ok = await _identity.DeleteUserByIdAsync(request.Id);
		return ok ? new ServiceResponse(true, "User deleted") : new ServiceResponse(false, "User not found or failed to delete");
	}
}
