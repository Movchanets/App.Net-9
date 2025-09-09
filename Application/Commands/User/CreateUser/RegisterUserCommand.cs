using Application.DTOs;
using Application.ViewModels;
using MediatR;

namespace Application.Commands.User.CreateUser;

public record RegisterUserCommand(RegistrationVM data ) : IRequest<ServiceResponse>;