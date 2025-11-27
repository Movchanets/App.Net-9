using Application.DTOs;
using MediatR;

namespace Application.Commands.User.Profile.UpdatePhone;

public record UpdatePhoneCommand(System.Guid UserId, UpdatePhoneDto Data) : IRequest<ServiceResponse<UserDto>>;
