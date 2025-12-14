namespace Domain.Entities;

public class ProductTag : BaseEntity<Guid>
{
	public Guid ProductId { get; private set; }
	public virtual Product? Product { get; private set; }
	public Guid TagId { get; private set; }
	public virtual Tag? Tag { get; private set; }

	private ProductTag() { }

	private ProductTag(Guid productId, Guid tagId)
	{
		if (productId == Guid.Empty)
		{
			throw new ArgumentException("ProductId cannot be empty", nameof(productId));
		}

		if (tagId == Guid.Empty)
		{
			throw new ArgumentException("TagId cannot be empty", nameof(tagId));
		}

		Id = Guid.NewGuid();
		ProductId = productId;
		TagId = tagId;
	}

	public static ProductTag Create(Product product, Tag tag)
	{
		if (product is null)
		{
			throw new ArgumentNullException(nameof(product));
		}

		if (tag is null)
		{
			throw new ArgumentNullException(nameof(tag));
		}

		var productTag = new ProductTag(product.Id, tag.Id);
		productTag.Attach(product, tag);
		return productTag;
	}

	public void Attach(Product product, Tag tag)
	{
		Product = product ?? throw new ArgumentNullException(nameof(product));
		ProductId = product.Id;
		Tag = tag ?? throw new ArgumentNullException(nameof(tag));
		TagId = tag.Id;
	}
}