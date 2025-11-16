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
    Task<User> AddAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(User user);
    Task<IEnumerable<User>> GetAllAsync();
}
