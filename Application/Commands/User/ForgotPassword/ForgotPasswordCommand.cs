using Application.DTOs;
using MediatR;

namespace Application.Commands.User.ForgotPassword;

/// <summary>
/// Команда ініціації відновлення паролю. Origin передається з контролера.
/// </summary>
public sealed record ForgotPasswordCommand(string Email, string Origin, string? TurnstileToken = null) : IRequest;
