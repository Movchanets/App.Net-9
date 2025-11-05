using Application.DTOs;
using MediatR;

namespace Application.Queries.User.GetUserById;

public record GetUserQuery(long Id) : IRequest<UserDto?>;