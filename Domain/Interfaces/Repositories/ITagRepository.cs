using Domain.Entities;

namespace Domain.Interfaces.Repositories;

/// <summary>
/// Repository для роботи з тегами
/// </summary>
public interface ITagRepository
{
	Task<IEnumerable<Tag>> GetAllAsync();
	Task<Tag?> GetByIdAsync(Guid id);
	Task<Tag?> GetBySlugAsync(string slug);

	void Add(Tag tag);
	void Update(Tag tag);
	void Delete(Tag tag);
}
