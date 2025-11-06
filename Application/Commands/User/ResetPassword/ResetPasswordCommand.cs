using Application.DTOs;
using MediatR;

namespace Application.Commands.User.ResetPassword;

public sealed record ResetPasswordCommand(ResetPasswordRequest Request) : IRequest;
