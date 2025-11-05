using Application.ViewModels;
using Infrastructure.Data.Models;
using Infrastructure.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Queries.User.GetUserByEmail;

/// <summary>
/// Handler для отримання користувача за email
/// </summary>
public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, ServiceResponse<UserVM>>
{
    private readonly UserManager<UserEntity> _userManager;

    /// <summary>
    /// Ініціалізує новий екземпляр GetUserByEmailQueryHandler
    /// </summary>
    public GetUserByEmailQueryHandler(UserManager<UserEntity> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>
    /// Обробляє запит отримання користувача за email
    /// </summary>
    /// <param name="request">Запит з email користувача</param>
    /// <param name="cancellationToken">Токен скасування</param>
    /// <returns>Дані користувача з ролями</returns>
    public async Task<ServiceResponse<UserVM>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return new ServiceResponse<UserVM>(false, "User not found");

        var roles = await _userManager.GetRolesAsync(user);

        var userVM = new UserVM
        {
            Id = user.Id,
            UserName = user?.UserName,
            Email = user?.Email,
            UserRoles = roles.ToList()
        };

        return new ServiceResponse<UserVM>(true, "User retrieved successfully", userVM);
    }
}