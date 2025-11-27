using Application.DTOs;
using MediatR;

namespace Application.Queries.User.GetUserByEmail;

public record GetUserByEmailQuery(string Email) : IRequest<ServiceResponse<UserDto>>;
