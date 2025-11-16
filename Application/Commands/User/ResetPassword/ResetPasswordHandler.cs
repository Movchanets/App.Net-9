using Application.DTOs;
using MediatR;
using Application.Interfaces;

namespace Application.Commands.User.ResetPassword;

public sealed class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand>
{
	private readonly IUserService _identity;

	public ResetPasswordHandler(IUserService identity)
	{
		_identity = identity;
	}

	public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
	{
		var r = request.Request;
		var ok = await _identity.ResetPasswordAsync(r.Email, r.Token, r.NewPassword);
		if (!ok)
			throw new InvalidOperationException("Invalid token or email.");

		return;
	}
}
