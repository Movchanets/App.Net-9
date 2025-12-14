namespace Domain.Entities;

public class ProductCategory : BaseEntity<Guid>
{
	public Guid ProductId { get; private set; }
	public virtual Product? Product { get; private set; }
	public Guid CategoryId { get; private set; }
	public virtual Category? Category { get; private set; }

	private ProductCategory() { }

	private ProductCategory(Guid productId, Guid categoryId)
	{
		if (productId == Guid.Empty)
		{
			throw new ArgumentException("ProductId cannot be empty", nameof(productId));
		}

		if (categoryId == Guid.Empty)
		{
			throw new ArgumentException("CategoryId cannot be empty", nameof(categoryId));
		}

		Id = Guid.NewGuid();
		ProductId = productId;
		CategoryId = categoryId;
	}

	public static ProductCategory Create(Product product, Category category)
	{
		if (product is null)
		{
			throw new ArgumentNullException(nameof(product));
		}

		if (category is null)
		{
			throw new ArgumentNullException(nameof(category));
		}

		var link = new ProductCategory(product.Id, category.Id);
		link.Attach(product, category);
		return link;
	}

	public void Attach(Product product, Category category)
	{
		Product = product ?? throw new ArgumentNullException(nameof(product));
		ProductId = product.Id;
		Category = category ?? throw new ArgumentNullException(nameof(category));
		CategoryId = category.Id;
	}
}