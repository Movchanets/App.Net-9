using Application.ViewModels;
using Infrastructure.Data.Constants;
using Infrastructure.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Commands.User.CreateUser;

/// <summary>
/// Handler для реєстрації нового користувача
/// </summary>
public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, ServiceResponse>
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly RoleManager<RoleEntity> _roleManager;

    /// <summary>
    /// Ініціалізує новий екземпляр RegisterUserHandler
    /// </summary>
    public RegisterUserHandler( UserManager<UserEntity> userManager, RoleManager<RoleEntity> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <summary>
    /// Обробляє команду реєстрації користувача
    /// </summary>
    /// <param name="request">Команда з даними користувача</param>
    /// <param name="cancellationToken">Токен скасування</param>
    /// <returns>Результат операції</returns>
    public async Task<ServiceResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
       
      var user = new UserEntity
        {
            UserName = request.data.Username,
            Email = request.data.Email,
        };
        var result = await _userManager.CreateAsync(user, request.data.Password);
        if (!result.Succeeded)
            return new ServiceResponse(false, "User creation failed", result.Errors.Select(e => e.Description).ToList());
        if (!await _roleManager.RoleExistsAsync(Roles.User))
        {
            await _roleManager.CreateAsync(new RoleEntity { Name = Roles.User });
        }
        {
            var role = new RoleEntity { Name = Roles.User };
            await _roleManager.CreateAsync(role);
        }

        // Призначаємо роль користувачу
        await _userManager.AddToRoleAsync(user, Roles.User);
        return result.Succeeded ? new ServiceResponse(true, "User created successfully") : 
            new ServiceResponse(false, "User creation failed", result.Errors.Select(e => e.Description).ToList());
        
    }
}