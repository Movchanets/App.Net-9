using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : GenericRepository<UserEntity>, IUserRepository
{
    public UserRepository(AppDbContext db) : base(db)
    {
    }

    public async Task<UserEntity?> GetByUsernameAsync(string username)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.UserName == username);
    }
}