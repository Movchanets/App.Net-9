using MediatR;

namespace Application.Commands.User.SendTestEmail;

public sealed record SendTestEmailCommand(string Email, string Origin) : IRequest;
