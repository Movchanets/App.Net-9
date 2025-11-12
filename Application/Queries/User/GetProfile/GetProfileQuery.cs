using MediatR;
using Application.DTOs;
using Application.ViewModels;

namespace Application.Queries.User.GetProfile;

public record GetProfileQuery(long UserId) : IRequest<ServiceResponse<UserDto>>;
