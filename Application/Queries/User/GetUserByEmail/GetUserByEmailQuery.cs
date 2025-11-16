using Application.ViewModels;
using Application.Models;
using MediatR;

namespace Application.Queries.User.GetUserByEmail;

public record GetUserByEmailQuery(string Email) : IRequest<ServiceResponse<UserVM>>;
