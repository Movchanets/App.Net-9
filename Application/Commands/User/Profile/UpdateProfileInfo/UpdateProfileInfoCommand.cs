using Application.DTOs;
using MediatR;

namespace Application.Commands.User.Profile.UpdateProfileInfo;

public record UpdateProfileInfoCommand(System.Guid UserId, UpdateProfileInfoDto Data) : IRequest<ServiceResponse<UserDto>>;
