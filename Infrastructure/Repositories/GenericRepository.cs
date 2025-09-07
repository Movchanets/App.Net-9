using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AppDbContext _db;

    public GenericRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<T> AddAsync(T entity)
    {
        _db.Set<T>().Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(T entity)
    {
        _db.Set<T>().Remove(entity);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _db.Set<T>().Where(predicate).ToListAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _db.Set<T>().ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _db.Set<T>().FindAsync(id);
    }

    public async Task<T> UpdateAsync(T entity)
    {
        _db.Set<T>().Update(entity);
        await _db.SaveChangesAsync();
        return entity;
    }
}