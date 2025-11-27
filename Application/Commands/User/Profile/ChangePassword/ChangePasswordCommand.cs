using Application.DTOs;
using MediatR;

namespace Application.Commands.User.Profile.ChangePassword;

public record ChangePasswordCommand(System.Guid UserId, ChangePasswordDto Data) : IRequest<ServiceResponse>;
