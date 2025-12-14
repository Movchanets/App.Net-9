using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Domain.Entities;

public class SkuEntity : BaseEntity<Guid>
{
	public Guid ProductId { get; private set; }
	public virtual Product? Product { get; private set; }
	public string SkuCode { get; private set; } = string.Empty;
	public decimal Price { get; private set; }
	public int StockQuantity { get; private set; }
	public JsonDocument? Attributes { get; private set; }

	private SkuEntity() { }

	private SkuEntity(Guid productId, decimal price, int stockQuantity, JsonDocument? attributes)
	{
		if (productId == Guid.Empty)
		{
			throw new ArgumentException("ProductId cannot be empty", nameof(productId));
		}

		if (price < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative");
		}

		if (stockQuantity < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(stockQuantity), "Stock quantity cannot be negative");
		}

		Id = Guid.NewGuid();
		ProductId = productId;
		Price = price;
		StockQuantity = stockQuantity;
		Attributes = attributes;
		SkuCode = GenerateSkuCode(attributes);
	}

	public static SkuEntity Create(Guid productId, decimal price, int stockQuantity, IDictionary<string, object?>? attributes = null)
	{
		var document = attributes is not null ? SerializeAttributes(attributes) : null;
		return new SkuEntity(productId, price, stockQuantity, document);
	}

	public void AttachProduct(Product product)
	{
		Product = product ?? throw new ArgumentNullException(nameof(product));
		ProductId = product.Id;
		MarkAsUpdated();
	}

	public void UpdatePrice(decimal price)
	{
		if (price < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative");
		}

		Price = price;
		MarkAsUpdated();
	}

	public void UpdateStock(int stockQuantity)
	{
		if (stockQuantity < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(stockQuantity), "Stock quantity cannot be negative");
		}

		StockQuantity = stockQuantity;
		MarkAsUpdated();
	}

	public void SetAttribute<T>(string key, T value)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			throw new ArgumentNullException(nameof(key));
		}

		var attributes = ToMutableAttributes();
		attributes[key] = value;
		ReplaceAttributes(attributes);
	}

	public T? GetAttribute<T>(string key)
	{
		if (string.IsNullOrWhiteSpace(key) || Attributes is null)
		{
			return default;
		}

		if (!Attributes.RootElement.TryGetProperty(key, out var element))
		{
			return default;
		}

		return element.ValueKind == JsonValueKind.Undefined || element.ValueKind == JsonValueKind.Null
			? default
			: element.Deserialize<T>();
	}

	public static string GenerateSkuCode(JsonDocument? attributes)
	{
		var canonical = BuildCanonicalAttributesString(attributes);
		var hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
		return Convert.ToHexString(hash.AsSpan(0, 8));
	}

	private static JsonDocument SerializeAttributes(IDictionary<string, object?> attributes)
	{
		var payload = JsonSerializer.Serialize(attributes);
		return JsonDocument.Parse(payload);
	}

	private void ReplaceAttributes(IDictionary<string, object?> attributes)
	{
		Attributes?.Dispose();
		Attributes = SerializeAttributes(attributes);
		SkuCode = GenerateSkuCode(Attributes);
		MarkAsUpdated();
	}

	private Dictionary<string, object?> ToMutableAttributes()
	{
		if (Attributes is null)
		{
			return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
		}

		var dictionary = JsonSerializer.Deserialize<Dictionary<string, object?>>(Attributes.RootElement.GetRawText());
		return dictionary ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
	}

	private static string BuildCanonicalAttributesString(JsonDocument? attributes)
	{
		if (attributes is null || attributes.RootElement.ValueKind != JsonValueKind.Object)
		{
			return "default";
		}

		var builder = new StringBuilder();
		foreach (var property in attributes.RootElement.EnumerateObject().OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase))
		{
			builder.Append(property.Name);
			builder.Append(':');
			builder.Append(property.Value.ToString());
			builder.Append('|');
		}

		return builder.Length == 0 ? "default" : builder.ToString();
	}
}