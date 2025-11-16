using Application.ViewModels;
using MediatR;

namespace Application.Commands.User.RegisterUser;

public record RegisterUserCommand(RegistrationVM data ) : IRequest<ServiceResponse>;