using Application.DTOs;
using MediatR;

namespace Application.Commands.User.Profile.UpdateEmail;

public record UpdateEmailCommand(System.Guid UserId, UpdateEmailDto Data) : IRequest<ServiceResponse<UserDto>>;
