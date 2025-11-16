using System.Linq.Expressions;
using Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Generic репозиторій для роботи з сутностями БД
/// </summary>
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AppDbContext _db;

    /// <summary>
    /// Ініціалізує новий екземпляр GenericRepository
    /// </summary>
    protected GenericRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Додає нову сутність в БД
    /// </summary>
    public async Task<T> AddAsync(T entity)
    {
        _db.Set<T>().Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Видаляє сутність з БД
    /// </summary>
    public async Task DeleteAsync(T entity)
    {
        _db.Set<T>().Remove(entity);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Знаходить сутності за предикатом
    /// </summary>
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _db.Set<T>().Where(predicate).ToListAsync();
    }

    /// <summary>
    /// Отримує всі сутності
    /// </summary>
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _db.Set<T>().ToListAsync();
    }

    /// <summary>
    /// Отримує сутність за ID
    /// </summary>
    public async Task<T?> GetByIdAsync(long id)
    {
        return await _db.Set<T>().FindAsync(id);
    }

    /// <summary>
    /// Оновлює сутність в БД
    /// </summary>
    public async Task<T> UpdateAsync(T entity)
    {
        _db.Set<T>().Update(entity);
        await _db.SaveChangesAsync();
        return entity;
    }
}