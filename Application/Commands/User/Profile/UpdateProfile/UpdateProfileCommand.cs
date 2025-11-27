using Application.DTOs;
using MediatR;

namespace Application.Commands.User.Profile.UpdateProfile;

public record UpdateProfileCommand(System.Guid UserId, UpdateProfileDto Data) : IRequest<ServiceResponse<UserDto>>;
