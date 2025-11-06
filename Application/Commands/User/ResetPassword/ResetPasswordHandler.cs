using Application.DTOs;
using Infrastructure.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Commands.User.ResetPassword;

public sealed class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand>
{
	private readonly UserManager<UserEntity> _userManager;

	public ResetPasswordHandler(UserManager<UserEntity> userManager)
	{
		_userManager = userManager;
	}

	public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
	{
		var r = request.Request;
		var user = await _userManager.FindByEmailAsync(r.Email);
		if (user == null)
			throw new InvalidOperationException("Invalid token or email.");

		var result = await _userManager.ResetPasswordAsync(user, r.Token, r.NewPassword);
		if (!result.Succeeded)
			throw new InvalidOperationException(string.Join(',', result.Errors.Select(e => e.Description)));

		return;
	}
}
