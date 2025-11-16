using Application.DTOs;
using Application.ViewModels;
using MediatR;

namespace Application.Commands.User.Profile.UpdateProfileInfo;

public record UpdateProfileInfoCommand(System.Guid UserId, UpdateProfileInfoVM Data) : IRequest<ServiceResponse<UserDto>>;
