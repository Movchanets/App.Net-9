using Application.ViewModels;
using MediatR;

namespace Application.Commands.User.Profile.ChangePassword;

public record ChangePasswordCommand(System.Guid UserId, ChangePasswordVM Data) : IRequest<ServiceResponse>;
