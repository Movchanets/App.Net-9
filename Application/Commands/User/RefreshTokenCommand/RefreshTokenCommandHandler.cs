using Application.Interfaces;
using Infrastructure.Data.Models;
using Infrastructure.Repositories.Interfaces;

namespace Application.Commands.User.RefreshTokenCommand;

using MediatR;

/// <summary>
/// Handler для оновлення токенів
/// </summary>
public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, TokenResponse>
{
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Ініціалізує новий екземпляр RefreshTokenCommandHandler
    /// </summary>
    public RefreshTokenCommandHandler(ITokenService tokenService, IUserRepository userRepository)
    {
        _tokenService = tokenService;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Обробляє команду оновлення токенів
    /// </summary>
    /// <param name="request">Команда з refresh token</param>
    /// <param name="cancellationToken">Токен скасування</param>
    /// <returns>Нові access та refresh токени</returns>
    public async Task<TokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByRefreshTokenAsync(request.RefreshToken);

        if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _userRepository.UpdateUserAsync(user);

        return new TokenResponse(newAccessToken, newRefreshToken);
    }
}
