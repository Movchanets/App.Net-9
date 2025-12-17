using Domain.Entities;
using Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Repository для роботи з тегами
/// </summary>
public class TagRepository : ITagRepository
{
	private readonly AppDbContext _db;

	public TagRepository(AppDbContext db)
	{
		_db = db;
	}

	public async Task<IEnumerable<Tag>> GetAllAsync()
	{
		return await _db.Tags
			.Include(t => t.ProductTags)
			.ToListAsync();
	}

	public async Task<Tag?> GetByIdAsync(Guid id)
	{
		return await _db.Tags
			.Include(t => t.ProductTags)
			.FirstOrDefaultAsync(t => t.Id == id);
	}

	public async Task<Tag?> GetBySlugAsync(string slug)
	{
		if (string.IsNullOrWhiteSpace(slug))
		{
			return null;
		}

		var normalized = slug.Trim();
		return await _db.Tags
			.Include(t => t.ProductTags)
			.FirstOrDefaultAsync(t => t.Slug == normalized);
	}

	public void Add(Tag tag)
	{
		_db.Tags.Add(tag);
	}

	public void Update(Tag tag)
	{
		_db.Tags.Update(tag);
	}

	public void Delete(Tag tag)
	{
		_db.Tags.Remove(tag);
	}
}
