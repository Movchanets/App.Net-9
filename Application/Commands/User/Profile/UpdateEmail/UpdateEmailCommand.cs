using Application.DTOs;
using Application.ViewModels;
using MediatR;

namespace Application.Commands.User.Profile.UpdateEmail;

public record UpdateEmailCommand(System.Guid UserId, UpdateEmailVM Data) : IRequest<ServiceResponse<UserDto>>;
