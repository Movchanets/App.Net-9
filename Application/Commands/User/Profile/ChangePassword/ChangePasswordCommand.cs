using Application.ViewModels;
using MediatR;

namespace Application.Commands.User.Profile.ChangePassword;

public record ChangePasswordCommand(long UserId, ChangePasswordVM Data) : IRequest<ServiceResponse>;
