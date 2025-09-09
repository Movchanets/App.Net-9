using Application.ViewModels;
using MediatR;

namespace Application.Commands.User.Queries.GetUserByEmail;

public record GetUserByEmailQuery(string Email) : IRequest<ServiceResponse>;
