using Application.DTOs;
using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using MediatR;

namespace Application.Commands.User.AuthenticateUser;

/// <summary>
/// Handler для логіну користувача
/// </summary>
public sealed class AuthenticateUserHandler : IRequestHandler<AuthenticateUserCommand, TokenResponse>
{
	private readonly IUserService _identityService;
	private readonly ITokenService _tokenService;

	public AuthenticateUserHandler(IUserService identityService, ITokenService tokenService)
	{
		_identityService = identityService;
		_tokenService = tokenService;
	}

	public async Task<TokenResponse> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
	{
		var dto = request.Request;
		// Turnstile token is validated by API filter when present.

		var valid = await _identityService.ValidatePasswordAsync(dto.Email, dto.Password);
		if (!valid)
			throw new UnauthorizedAccessException("Invalid credentials");

		Domain.Entities.User? domainUser = await _identityService.FindUserByEmailAsync(dto.Email);
		if (domainUser is null)
			throw new UnauthorizedAccessException("User not found");

		return await _tokenService.GenerateTokensAsync(domainUser.IdentityUserId);
	}
}
