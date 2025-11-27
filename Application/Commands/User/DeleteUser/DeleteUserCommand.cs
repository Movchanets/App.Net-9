using Application.DTOs;
using MediatR;

namespace Application.Commands.User.DeleteUser;

public record DeleteUserCommand(System.Guid Id) : IRequest<ServiceResponse>;
