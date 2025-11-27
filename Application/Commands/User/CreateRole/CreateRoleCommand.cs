using Application.DTOs;
using MediatR;

namespace Application.Commands.User;

public class CreateRoleCommand : IRequest<ServiceResponse>
{
    public string RoleName { get; init; }
}
