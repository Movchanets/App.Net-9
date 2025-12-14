namespace Domain.Entities;

public class ProductGallery : BaseEntity<Guid>
{
	public Guid ProductId { get; private set; }
	public virtual Product? Product { get; private set; }
	public Guid MediaImageId { get; private set; }
	public virtual MediaImage? MediaImage { get; private set; }
	public int DisplayOrder { get; private set; }

	private ProductGallery() { }

	private ProductGallery(Guid productId, Guid mediaImageId, int displayOrder)
	{
		if (productId == Guid.Empty)
		{
			throw new ArgumentException("ProductId cannot be empty", nameof(productId));
		}

		if (mediaImageId == Guid.Empty)
		{
			throw new ArgumentException("MediaImageId cannot be empty", nameof(mediaImageId));
		}

		if (displayOrder < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(displayOrder), "DisplayOrder cannot be negative");
		}

		Id = Guid.NewGuid();
		ProductId = productId;
		MediaImageId = mediaImageId;
		DisplayOrder = displayOrder;
	}

	public static ProductGallery Create(Product product, MediaImage mediaImage, int displayOrder = 0)
	{
		if (product is null)
		{
			throw new ArgumentNullException(nameof(product));
		}

		if (mediaImage is null)
		{
			throw new ArgumentNullException(nameof(mediaImage));
		}

		var galleryItem = new ProductGallery(product.Id, mediaImage.Id, displayOrder);
		galleryItem.Attach(product, mediaImage);
		return galleryItem;
	}

	public void SetDisplayOrder(int displayOrder)
	{
		if (displayOrder < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(displayOrder), "DisplayOrder cannot be negative");
		}

		DisplayOrder = displayOrder;
		MarkAsUpdated();
	}

	public void Attach(Product product, MediaImage mediaImage)
	{
		Product = product ?? throw new ArgumentNullException(nameof(product));
		ProductId = product.Id;
		MediaImage = mediaImage ?? throw new ArgumentNullException(nameof(mediaImage));
		MediaImageId = mediaImage.Id;
	}
}