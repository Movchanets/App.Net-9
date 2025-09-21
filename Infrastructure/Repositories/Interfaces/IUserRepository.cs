using Infrastructure.Entities;

namespace Infrastructure.Repositories.Interfaces;

public interface IUserRepository : IGenericRepository<UserEntity>
{
    Task<UserEntity?> GetByUsernameAsync(string username);
    Task<UserEntity?> GetUserByRefreshTokenAsync(string refreshToken);
    Task<bool> UpdateUserAsync (UserEntity user);
}