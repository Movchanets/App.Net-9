using Domain.Entities;
using Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Repository для роботи з MediaImage
/// </summary>
public class MediaImageRepository : IMediaImageRepository
{
	private readonly AppDbContext _db;

	public MediaImageRepository(AppDbContext db)
	{
		_db = db;
	}

	/// <summary>
	/// Отримує MediaImage за ID
	/// </summary>
	public async Task<MediaImage?> GetByIdAsync(Guid id)
	{
		return await _db.MediaImages
			.Include(m => m.Product)
			.FirstOrDefaultAsync(m => m.Id == id);
	}

	/// <summary>
	/// Отримує MediaImage за ключем сховища
	/// </summary>
	public async Task<MediaImage?> GetByStorageKeyAsync(string storageKey)
	{
		return await _db.MediaImages
			.Include(m => m.Product)
			.FirstOrDefaultAsync(m => m.StorageKey == storageKey);
	}

	/// <summary>
	/// Додає нове MediaImage в БД
	/// </summary>
	public void Add(MediaImage mediaImage)
	{
		_db.MediaImages.Add(mediaImage);
	}

	/// <summary>
	/// Оновлює MediaImage в БД
	/// </summary>
	public void Update(MediaImage mediaImage)
	{
		_db.MediaImages.Update(mediaImage);
	}

	/// <summary>
	/// Видаляє MediaImage з БД
	/// </summary>
	public void Delete(MediaImage mediaImage)
	{
		_db.MediaImages.Remove(mediaImage);
	}

	/// <summary>
	/// Отримує всі MediaImage
	/// </summary>
	public async Task<IEnumerable<MediaImage>> GetAllAsync()
	{
		return await _db.MediaImages
			.Include(m => m.Product)
			.ToListAsync();
	}

	/// <summary>
	/// Отримує всі MediaImage для конкретного продукту
	/// </summary>
	public async Task<IEnumerable<MediaImage>> GetByProductIdAsync(Guid productId)
	{
		return await _db.MediaImages
			.Where(m => m.ProductId == productId)
			.ToListAsync();
	}

	/// <summary>
	/// Отримує MediaImage, які не прив'язані до жодного продукту або користувача
	/// (orphaned images - можуть бути видалені)
	/// </summary>
	public async Task<IEnumerable<MediaImage>> GetOrphanedImagesAsync()
	{
		// Знаходимо зображення, які не прив'язані до продукту
		var orphanedProductImages = await _db.MediaImages
			.Where(m => m.ProductId == null)
			.ToListAsync();

		// Фільтруємо зображення, які не використовуються як аватари
		var usedAvatarIds = await _db.DomainUsers
			.Where(u => u.AvatarId != null)
			.Select(u => u.AvatarId!.Value)
			.ToListAsync();

		return orphanedProductImages
			.Where(m => !usedAvatarIds.Contains(m.Id))
			.ToList();
	}
}
