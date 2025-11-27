using Application.DTOs;
using Application.Interfaces;
using Domain.Constants;
using MediatR;

namespace Application.Commands.User.RegisterUser;

/// <summary>
/// Handler для реєстрації нового користувача
/// </summary>
public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, ServiceResponse>
{
    private readonly IUserService _identity;

    /// <summary>
    /// Ініціалізує новий екземпляр RegisterUserHandler
    /// </summary>
    public RegisterUserHandler(IUserService identity)
    {
        _identity = identity;
    }
    /// <summary>
    /// Генерує унікальне ім'я користувача на основі імені та прізвища.
    /// </summary>
    /// <param name="firstName">Ім'я</param>
    /// <param name="lastName">Прізвище</param>
    /// <returns>Унікальне ім'я користувача</returns>
    private Task<string> GenerateUniqueUsername(string firstName, string lastName) => Task.FromResult(string.Empty);
    /// <summary>
    /// Обробляє команду реєстрації користувача
    /// </summary>
    /// <param name="request">Команда з даними користувача</param>
    /// <param name="cancellationToken">Токен скасування</param>
    /// <returns>Результат операції</returns>
    public async Task<ServiceResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Turnstile token validated by API filter when present.

        var res = await _identity.RegisterAsync(request.data);
        if (!res.Succeeded)
            return new ServiceResponse(false, "User creation failed", res.Errors);
        await _identity.EnsureRoleExistsAsync(Roles.User);
        if (res.IdentityUserId.HasValue)
            await _identity.AddUserToRoleAsync(res.IdentityUserId.Value, Roles.User);
        return new ServiceResponse(true, "User created successfully");

    }
}