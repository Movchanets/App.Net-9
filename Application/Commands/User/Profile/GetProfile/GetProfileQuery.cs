using MediatR;
using Application.DTOs;
using Application.ViewModels;

namespace Application.Commands.User.Profile.GetProfile;

public record GetProfileQuery(long UserId) : IRequest<ServiceResponse<UserDto>>;
