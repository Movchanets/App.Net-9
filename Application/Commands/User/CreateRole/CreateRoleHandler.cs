using Application.ViewModels;
using MediatR;
using Application.Interfaces;

namespace Application.Commands.User.CreateRole;

/// <summary>
/// Handler для створення нової ролі
/// </summary>
public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, ServiceResponse>
{
    private readonly IUserService _identity;

    /// <summary>
    /// Ініціалізує новий екземпляр CreateRoleCommandHandler
    /// </summary>
    public CreateRoleCommandHandler(IUserService identity)
    {
        _identity = identity;
    }

    /// <summary>
    /// Обробляє команду створення ролі
    /// </summary>
    /// <param name="request">Команда з назвою ролі</param>
    /// <param name="cancellationToken">Токен скасування</param>
    /// <returns>Результат операції</returns>
    public async Task<ServiceResponse> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var created = await _identity.CreateRoleAsync(request.RoleName);
        return created
            ? new ServiceResponse(true, "Role created successfully")
            : new ServiceResponse(false, "Role creation failed");
    }
}
