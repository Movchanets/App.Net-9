using Application.DTOs;
using Application.Models;
using MediatR;

namespace Application.Commands.User.AuthenticateUser;

public sealed record AuthenticateUserCommand(LoginRequest Request) : IRequest<TokenResponse>;
