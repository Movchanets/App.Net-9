using Application.DTOs;
using Application.ViewModels;
using MediatR;

namespace Application.Commands.User.Profile.UpdatePhone;

public record UpdatePhoneCommand(System.Guid UserId, UpdatePhoneVM Data) : IRequest<ServiceResponse<UserDto>>;
