using Application.ViewModels;
using Application.Models;
using MediatR;
using Application.Interfaces;

namespace Application.Queries.User.GetUserByEmail;

/// <summary>
/// Handler для отримання користувача за email
/// </summary>
public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, ServiceResponse<UserVM>>
{
    private readonly IUserService _identity;

    /// <summary>
    /// Ініціалізує новий екземпляр GetUserByEmailQueryHandler
    /// </summary>
    public GetUserByEmailQueryHandler(IUserService identity)
    {
        _identity = identity;
    }

    /// <summary>
    /// Обробляє запит отримання користувача за email
    /// </summary>
    /// <param name="request">Запит з email користувача</param>
    /// <param name="cancellationToken">Токен скасування</param>
    /// <returns>Дані користувача з ролями</returns>
    public async Task<ServiceResponse<UserVM>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var info = await _identity.GetIdentityInfoByEmailAsync(request.Email);
        if (info == null)
            return new ServiceResponse<UserVM>(false, "User not found");

        var userVM = new UserVM
        {
            Id = info.Id,
            UserName = info.Username,
            Email = info.Email,
            UserRoles = info.Roles
        };

        return new ServiceResponse<UserVM>(true, "User retrieved successfully", userVM);
    }
}