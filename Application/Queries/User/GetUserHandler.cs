using Application.DTOs;
using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Application.Queries.User;

/// <summary>
/// Handler для отримання користувача за ID
/// </summary>
public class GetUserHandler : IRequestHandler<GetUserQuery, UserDto?>
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _repository;

    /// <summary>
    /// Ініціалізує новий екземпляр GetUserHandler
    /// </summary>
    public GetUserHandler(IUserRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <summary>
    /// Обробляє запит отримання користувача за ID
    /// </summary>
    /// <param name="request">Запит з ID користувача</param>
    /// <param name="cancellationToken">Токен скасування</param>
    /// <returns>DTO користувача або null, якщо не знайдено</returns>
    public async Task<UserDto?> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(request.Id);
        return user == null ? null : _mapper.Map<UserDto>(user);
    }
}