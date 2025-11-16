using Application.Interfaces;
using Application.Models;

namespace Application.Commands.User.RefreshTokenCommand;

using MediatR;

/// <summary>
/// Handler для оновлення токенів
/// </summary>
public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, TokenResponse>
{
    private readonly ITokenService _tokenService;
    private readonly IUserService _identityService;

    /// <summary>
    /// Ініціалізує новий екземпляр RefreshTokenCommandHandler
    /// </summary>
    public RefreshTokenCommandHandler(ITokenService tokenService, IUserService identityService)
    {
        _tokenService = tokenService;
        _identityService = identityService;
    }

    /// <summary>
    /// Обробляє команду оновлення токенів
    /// </summary>
    /// <param name="request">Команда з refresh token</param>
    /// <param name="cancellationToken">Токен скасування</param>
    /// <returns>Нові access та refresh токени</returns>
    public async Task<TokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var identityUserId = await _identityService.ValidateRefreshTokenAsync(request.RefreshToken);
        if (!identityUserId.HasValue)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        var newAccessToken = await _tokenService.GenerateAccessTokenAsync(identityUserId.Value);
        var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(identityUserId.Value);

        return new TokenResponse(newAccessToken, newRefreshToken);
    }
}
