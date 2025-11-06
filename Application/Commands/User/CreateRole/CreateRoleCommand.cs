using Application.ViewModels;
using MediatR;

namespace Application.Commands.User;

public class CreateRoleCommand : IRequest<ServiceResponse>
{
    public string RoleName { get; init; }
}
