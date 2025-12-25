using Domain.Helpers;

namespace Domain.Entities;

public class Tag : BaseEntity<Guid>
{
	private readonly List<ProductTag> _productTags = new();

	public string Name { get; private set; }
	public string Slug { get; private set; }
	public string? Description { get; private set; }
	public virtual IReadOnlyCollection<ProductTag> ProductTags => _productTags.AsReadOnly();

	private Tag() { }

	private Tag(string name, string? description)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentNullException(nameof(name));
		}

		Id = Guid.NewGuid();
		Name = name.Trim();
		Description = description?.Trim();
		Slug = SlugHelper.GenerateSlug(name);
	}

	public static Tag Create(string name, string? description = null)
	{
		return new Tag(name, description);
	}

	public void Rename(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentNullException(nameof(name));
		}

		Name = name.Trim();
		Slug = SlugHelper.GenerateSlug(name);
		MarkAsUpdated();
	}

	public void UpdateDescription(string? description)
	{
		Description = description?.Trim();
		MarkAsUpdated();
	}

	public void Update(string name, string? description)
	{
		Rename(name);
		UpdateDescription(description);
	}

	internal void AddProductTag(ProductTag productTag)
	{
		if (productTag is null)
		{
			throw new ArgumentNullException(nameof(productTag));
		}

		if (_productTags.Contains(productTag))
		{
			return;
		}

		_productTags.Add(productTag);
	}

	internal void RemoveProductTag(ProductTag productTag)
	{
		if (productTag is null)
		{
			throw new ArgumentNullException(nameof(productTag));
		}

		_productTags.Remove(productTag);
	}

}