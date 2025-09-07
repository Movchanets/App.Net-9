using Application.DTOs;
using MediatR;

namespace Application.Queries.User;

public record GetUserQuery(int Id) : IRequest<UserDto?>;