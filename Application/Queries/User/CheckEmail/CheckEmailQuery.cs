using Application.DTOs;
using MediatR;

namespace Application.Queries.User.CheckEmail;

public sealed record CheckEmailQuery(string Email) : IRequest<CheckEmailResponse>;
