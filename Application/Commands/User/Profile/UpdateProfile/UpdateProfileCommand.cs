using Application.ViewModels;
using Application.DTOs;
using MediatR;

namespace Application.Commands.User.Profile.UpdateProfile;

public record UpdateProfileCommand(System.Guid UserId, UpdateProfileVM Data) : IRequest<ServiceResponse<UserDto>>;
