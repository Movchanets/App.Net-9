using Application.DTOs;
using Infrastructure.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Queries.User.CheckEmail;

public sealed class CheckEmailHandler : IRequestHandler<CheckEmailQuery, CheckEmailResponse>
{
	private readonly UserManager<UserEntity> _userManager;

	public CheckEmailHandler(UserManager<UserEntity> userManager)
	{
		_userManager = userManager;
	}

	public async Task<CheckEmailResponse> Handle(CheckEmailQuery request, CancellationToken cancellationToken)
	{
		var user = await _userManager.FindByEmailAsync(request.Email);
		return new CheckEmailResponse(user != null);
	}
}
