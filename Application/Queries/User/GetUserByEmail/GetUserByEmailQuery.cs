using Application.ViewModels;
using Infrastructure.Data.Models;
using MediatR;

namespace Application.Queries.User.GetUserByEmail;

public record GetUserByEmailQuery(string Email) : IRequest<ServiceResponse<UserVM>>;
