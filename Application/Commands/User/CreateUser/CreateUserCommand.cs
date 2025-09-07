using Application.DTOs;
using MediatR;

namespace Application.Commands.User.CreateUser;

public record CreateUserCommand(string Username, string Email) : IRequest<UserDto>;