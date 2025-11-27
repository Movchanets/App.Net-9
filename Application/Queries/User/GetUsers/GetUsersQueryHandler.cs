using Application.DTOs;
using MediatR;
using Application.Interfaces;

namespace Application.Queries.User.GetUsers;

/// <summary>
/// Handler для отримання всіх користувачів
/// </summary>
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, ServiceResponse<List<UserDto>>>
{
    private readonly IUserService _userService;

    /// <summary>
    /// Ініціалізує новий екземпляр GetUsersQueryHandler
    /// </summary>
    public GetUsersQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Обробляє запит отримання всіх користувачів
    /// </summary>
    /// <param name="request">Запит</param>
    /// <param name="cancellationToken">Токен скасування</param>
    /// <returns>Список користувачів з ролями</returns>
    public async Task<ServiceResponse<List<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllUsersAsync();
        if (users.Count == 0)
            return new ServiceResponse<List<UserDto>>(false, "No users found");

        return new ServiceResponse<List<UserDto>>(true, "Users retrieved successfully", users);
    }
}