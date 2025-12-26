using Domain.Entities;

namespace Domain.Interfaces.Repositories;

/// <summary>
/// Repository для роботи з SKU
/// </summary>
public interface ISkuRepository
{
	Task<SkuEntity?> GetByIdAsync(Guid id);
	Task<SkuEntity?> GetBySkuCodeAsync(string skuCode);
	Task<IEnumerable<SkuEntity>> GetByProductIdAsync(Guid productId);
	Task<IEnumerable<SkuEntity>> GetByJsonAttributeAsync(string key, string value);

	Task<IEnumerable<SkuEntity>> SearchBySkuCodeAsync(string query, int take = 50);

	void Add(SkuEntity sku);
	void Update(SkuEntity sku);
	void Delete(SkuEntity sku);
}
