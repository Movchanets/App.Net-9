using MediatR;
using Application.ViewModels;

namespace Application.Commands.User.DeleteUser;

public record DeleteUserCommand(System.Guid Id) : IRequest<ServiceResponse>;
