using Application.DTOs;
using Application.ViewModels;
using MediatR;

namespace Application.Commands.User.Profile.DeleteProfilePicture;

public record DeleteProfilePictureCommand(System.Guid UserId) : IRequest<ServiceResponse<UserDto>>;
