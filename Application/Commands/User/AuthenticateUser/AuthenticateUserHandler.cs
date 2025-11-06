using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Data.Models;
using Infrastructure.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Commands.User.AuthenticateUser;

/// <summary>
/// Handler для логіну користувача
/// </summary>
public sealed class AuthenticateUserHandler : IRequestHandler<AuthenticateUserCommand, TokenResponse>
{
	private readonly UserManager<UserEntity> _userManager;
	private readonly ITokenService _tokenService;

	public AuthenticateUserHandler(UserManager<UserEntity> userManager, ITokenService tokenService)
	{
		_userManager = userManager;
		_tokenService = tokenService;
	}

	public async Task<TokenResponse> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
	{
		var dto = request.Request;
		var user = await _userManager.FindByEmailAsync(dto.Email);
		if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
			throw new UnauthorizedAccessException("Invalid credentials");

		return await _tokenService.GenerateTokensAsync(user);
	}
}
