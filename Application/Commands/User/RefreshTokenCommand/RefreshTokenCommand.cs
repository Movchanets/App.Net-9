using Application.Models;
using MediatR;

namespace Application.Commands.User.RefreshTokenCommand;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<TokenResponse>;
