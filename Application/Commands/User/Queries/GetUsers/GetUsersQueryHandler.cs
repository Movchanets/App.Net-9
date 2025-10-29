using Application.ViewModels;
using Infrastructure.Data.Models;
using Infrastructure.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Commands.User.Queries.GetUsers;

/// <summary>
/// Handler для отримання всіх користувачів
/// </summary>
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, ServiceResponse>
{
    private readonly UserManager<UserEntity> _userManager;

    /// <summary>
    /// Ініціалізує новий екземпляр GetUsersQueryHandler
    /// </summary>
    public GetUsersQueryHandler(UserManager<UserEntity> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>
    /// Обробляє запит отримання всіх користувачів
    /// </summary>
    /// <param name="request">Запит</param>
    /// <param name="cancellationToken">Токен скасування</param>
    /// <returns>Список користувачів з ролями</returns>
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
            return new ServiceResponse(false, "No users found");
        }
        return new ServiceResponse(true, "Users retrieved successfully", userVMs);
     
    }
}