using Application.DTOs;
using MediatR;

namespace Application.Commands.User.Profile.DeleteProfilePicture;

public record DeleteProfilePictureCommand(System.Guid UserId) : IRequest<ServiceResponse<UserDto>>;
