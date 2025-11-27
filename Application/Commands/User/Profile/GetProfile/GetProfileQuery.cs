using MediatR;
using Application.DTOs;

namespace Application.Commands.User.Profile.GetProfile;

public record GetProfileQuery(System.Guid UserId) : IRequest<ServiceResponse<UserDto>>;
