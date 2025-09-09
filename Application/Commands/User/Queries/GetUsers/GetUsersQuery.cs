using Application.ViewModels;
using MediatR;

namespace Application.Commands.User.Queries.GetUsers;


public record GetUsersQuery : IRequest<ServiceResponse>;