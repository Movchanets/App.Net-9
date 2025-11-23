using Application.DTOs;
using Application.ViewModels;
using MediatR;
using Application.Interfaces;

namespace Application.Queries.User.GetUserByEmail;

/// <summary>
/// Handler для отримання користувача за email
/// </summary>
public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, ServiceResponse<UserDto>>
{
    private readonly IUserService _userService;

    /// <summary>
    /// Ініціалізує новий екземпляр GetUserByEmailQueryHandler
    /// </summary>
    public GetUserByEmailQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Обробляє запит отримання користувача за email
    /// </summary>
    /// <param name="request">Запит з email користувача</param>
    /// <param name="cancellationToken">Токен скасування</param>
    /// <returns>Дані користувача з ролями</returns>
    public async Task<ServiceResponse<UserDto>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var userDto = await _userService.GetIdentityInfoByEmailAsync(request.Email);
        if (userDto == null)
            return new ServiceResponse<UserDto>(false, "User not found");

        return new ServiceResponse<UserDto>(true, "User retrieved successfully", userDto);
    }
}