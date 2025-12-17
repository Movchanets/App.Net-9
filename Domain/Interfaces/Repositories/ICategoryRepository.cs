using Domain.Entities;

namespace Domain.Interfaces.Repositories;

/// <summary>
/// Repository для роботи з категоріями
/// </summary>
public interface ICategoryRepository
{
	Task<Category?> GetByIdAsync(Guid id);
	Task<Category?> GetBySlugAsync(string slug);
	Task<IEnumerable<Category>> GetAllAsync();
	Task<IEnumerable<Category>> GetTopLevelAsync();
	Task<IEnumerable<Category>> GetSubCategoriesAsync(Guid parentCategoryId);

	void Add(Category category);
	void Update(Category category);
	void Delete(Category category);
}
