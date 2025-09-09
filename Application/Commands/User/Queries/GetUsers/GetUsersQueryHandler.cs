using Application.ViewModels;
using Infrastructure.Data.Models;
using Infrastructure.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Commands.User.Queries.GetUsers;


public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, ServiceResponse>
{
    private readonly UserManager<UserEntity> _userManager;

    public GetUsersQueryHandler(UserManager<UserEntity> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ServiceResponse> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = _userManager?.Users.ToList();
        var userVMs = new List<UserVM>();
        if (users != null)
        {
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userVM = new UserVM
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    UserRoles = roles.ToList()
                };
                userVMs.Add(userVM);
            }
        }
        else
        {
            return new ServiceResponse(false, "No users found", null);
        }
        return new ServiceResponse(true, "Users retrieved successfully", userVMs);
     
    }
}