using Application.ViewModels;
using Application.DTOs;
using MediatR;

namespace Application.Commands.User.Profile.UpdateProfile;

public record UpdateProfileCommand(long UserId, UpdateProfileVM Data) : IRequest<ServiceResponse<UserDto>>;
