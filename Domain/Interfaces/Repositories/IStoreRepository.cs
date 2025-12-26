using Domain.Entities;

namespace Domain.Interfaces.Repositories;

/// <summary>
/// Repository для роботи з магазинами
/// </summary>
public interface IStoreRepository
{
	Task<Store?> GetByIdAsync(Guid id);
	Task<Store?> GetByUserIdAsync(Guid userId);
	Task<Store?> GetBySlugAsync(string slug);
	Task<IEnumerable<Store>> GetAllAsync(bool includeUnverified = false);

	void Add(Store store);
	void Update(Store store);
	void Delete(Store store);
}
