using Application.DTOs;
using Application.ViewModels;
using MediatR;

namespace Application.Queries.User.GetUsers;

public record GetUsersQuery : IRequest<ServiceResponse<List<UserDto>>>;