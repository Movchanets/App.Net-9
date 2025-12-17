using Domain.Helpers;

namespace Domain.Entities;

public class Store : BaseEntity<Guid>
{
	private readonly List<Product> _products = new();
	public virtual IReadOnlyCollection<Product> Products => _products.AsReadOnly();

	public string Name { get; private set; } = string.Empty;
	public string Slug { get; private set; } = string.Empty;
	public string? Description { get; private set; }
	public Guid UserId { get; private set; }
	public virtual User? User { get; private set; }
	public bool IsVerified { get; private set; }
	public bool IsSuspended { get; private set; }

	private Store() { }

	private Store(Guid userId, string name, string? description)
	{
		if (userId == Guid.Empty)
		{
			throw new ArgumentException("UserId cannot be empty", nameof(userId));
		}

		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentNullException(nameof(name));
		}

		Id = Guid.NewGuid();
		UserId = userId;
		Name = name.Trim();
		Description = description?.Trim();
		Slug = SlugHelper.GenerateSlug(name);
		IsVerified = false;
		IsSuspended = false;
	}

	public static Store Create(Guid userId, string name, string? description = null)
	{
		return new Store(userId, name, description);
	}

	public void UpdateDetails(string name, string? description)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentNullException(nameof(name));
		}

		Name = name.Trim();
		Description = description?.Trim();
		Slug = SlugHelper.GenerateSlug(name);
		MarkAsUpdated();
	}

	public void Verify()
	{
		if (IsVerified)
		{
			return;
		}

		IsVerified = true;
		MarkAsUpdated();
	}

	public void Suspend()
	{
		if (IsSuspended)
		{
			return;
		}

		IsSuspended = true;
		MarkAsUpdated();
	}

	public void Unsuspend()
	{
		if (!IsSuspended)
		{
			return;
		}

		IsSuspended = false;
		MarkAsUpdated();
	}

	public void AddProduct(Product product)
	{
		if (product is null)
		{
			throw new ArgumentNullException(nameof(product));
		}

		if (_products.Any(p => p.Id == product.Id))
		{
			return;
		}

		product.AssignToStore(this);
		_products.Add(product);
		MarkAsUpdated();
	}

}