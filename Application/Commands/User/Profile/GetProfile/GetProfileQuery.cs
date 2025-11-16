using MediatR;
using Application.DTOs;
using Application.ViewModels;

namespace Application.Commands.User.Profile.GetProfile;

public record GetProfileQuery(System.Guid UserId) : IRequest<ServiceResponse<UserDto>>;
