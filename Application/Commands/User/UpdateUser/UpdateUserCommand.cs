using Application.ViewModels;
using Application.DTOs;
using MediatR;

namespace Application.Commands.User.UpdateUser;

public record UpdateUserCommand(long Id, UpdateUserVM Data) : IRequest<ServiceResponse<UserDto>>;
