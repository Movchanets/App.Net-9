using System;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Repository для роботи з доменними User
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _db.DomainUsers
            .Include(u => u.Avatar)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByIdentityUserIdAsync(Guid identityUserId)
    {
        return await _db.DomainUsers
            .Include(u => u.Avatar)
            .FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
    }

    public void Add(User user)
    {
        _db.DomainUsers.Add(user);
    }

    public void Update(User user)
    {
        _db.DomainUsers.Update(user);
    }

    public void Delete(User user)
    {
        _db.DomainUsers.Remove(user);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _db.DomainUsers
            .Include(u => u.Avatar)
            .ToListAsync();
    }
}
