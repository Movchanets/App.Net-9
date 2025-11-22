using System;
using Domain.Entities;

namespace Domain.Interfaces.Repositories;

/// <summary>
/// Repository для роботи з доменними користувачами
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByIdentityUserIdAsync(Guid identityUserId);
    void Add(User user);
    void Update(User user);
    void Delete(User user);
    Task<IEnumerable<User>> GetAllAsync();
}
