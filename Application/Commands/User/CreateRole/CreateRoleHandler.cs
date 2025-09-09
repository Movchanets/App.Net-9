using Application.ViewModels;
using Infrastructure.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Commands.User.CreateRole;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, ServiceResponse>
{
    private readonly RoleManager<RoleEntity> _roleManager;

    public CreateRoleCommandHandler(RoleManager<RoleEntity> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<ServiceResponse> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = new RoleEntity { Name = request.RoleName };

        var result = await _roleManager.CreateAsync(role);

        return result.Succeeded
            ? new ServiceResponse(true, "Role created successfully")
            : new ServiceResponse(false, "Role creation failed", result.Errors.Select(e => e.Description).ToList());
    }
}
