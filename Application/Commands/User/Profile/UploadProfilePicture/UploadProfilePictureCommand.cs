using Application.DTOs;
using Application.ViewModels;
using MediatR;

namespace Application.Commands.User.Profile.UploadProfilePicture;

public record UploadProfilePictureCommand(
	System.Guid UserId,
	Stream FileStream,
	string FileName,
	string ContentType
) : IRequest<ServiceResponse<UserDto>>;
