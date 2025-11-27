using MediatR;
using Application.DTOs;

namespace Application.Queries.User.GetProfile;

public record GetProfileQuery(System.Guid UserId) : IRequest<ServiceResponse<UserDto>>;
