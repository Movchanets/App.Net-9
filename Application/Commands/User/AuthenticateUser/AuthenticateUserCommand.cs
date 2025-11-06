using Application.DTOs;
using Infrastructure.Data.Models;
using MediatR;

namespace Application.Commands.User.AuthenticateUser;

public sealed record AuthenticateUserCommand(LoginRequest Request) : IRequest<TokenResponse>;
