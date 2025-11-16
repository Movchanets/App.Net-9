using MediatR;
using Application.DTOs;
using Application.ViewModels;

namespace Application.Queries.User.GetProfile;

public record GetProfileQuery(System.Guid UserId) : IRequest<ServiceResponse<UserDto>>;
