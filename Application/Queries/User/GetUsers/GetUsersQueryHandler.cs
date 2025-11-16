using Application.ViewModels;
using Application.Models;
using MediatR;
using Application.Interfaces;

namespace Application.Queries.User.GetUsers;

/// <summary>
/// Handler для отримання всіх користувачів
/// </summary>
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, ServiceResponse>
{
    private readonly IUserService _identity;

    /// <summary>
    /// Ініціалізує новий екземпляр GetUsersQueryHandler
    /// </summary>
    public GetUsersQueryHandler(IUserService identity)
    {
        _identity = identity;
    }

    /// <summary>
    /// Обробляє запит отримання всіх користувачів
    /// </summary>
    /// <param name="request">Запит</param>
    /// <param name="cancellationToken">Токен скасування</param>
    /// <returns>Список користувачів з ролями</returns>
    public async Task<ServiceResponse> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _identity.GetAllUsersAsync();
        if (users.Count == 0)
            return new ServiceResponse(false, "No users found");
        var userVMs = users.Select(u => new UserVM
        {
            Id = u.Id,
            UserName = u.Username,
            Email = u.Email,
            UserRoles = u.Roles
        }).ToList();
        return new ServiceResponse(true, "Users retrieved successfully", userVMs);

    }
}