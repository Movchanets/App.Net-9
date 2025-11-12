using MediatR;
using Application.ViewModels;

namespace Application.Commands.User.DeleteUser;

public record DeleteUserCommand(long Id) : IRequest<ServiceResponse>;
