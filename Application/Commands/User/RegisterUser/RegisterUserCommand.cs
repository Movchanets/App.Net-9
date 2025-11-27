using Application.DTOs;
using MediatR;

namespace Application.Commands.User.RegisterUser;

public record RegisterUserCommand(RegistrationDto data) : IRequest<ServiceResponse>;