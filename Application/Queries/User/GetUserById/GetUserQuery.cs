using Application.DTOs;
using MediatR;

namespace Application.Queries.User.GetUserById;

public record GetUserQuery(System.Guid Id) : IRequest<UserDto?>;