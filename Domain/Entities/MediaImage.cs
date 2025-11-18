namespace Domain.Entities;

public class MediaImage
{
    // EF Core потрібен Primary Key
    public Guid Id { get; private set; }

    public string StorageKey { get; private set; }
    public string MimeType { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public string? AltText { get; private set; } // AltText можна змінювати пізніше, тому тут можна зробити public set

    // --- Зв'язки ---
    
    // Для Продукту (One-to-Many)
    public Guid? ProductId { get; private set; } // Nullable, бо картинка може бути аватаром юзера
     public virtual Product? Product { get; private set; }

    // Приватний конструктор для EF Core (Materialization)
    private MediaImage() { }

    // Публічний конструктор для створення нової картинки
    public MediaImage(string storageKey, string mimeType, int width, int height, string? altText = null)
    {
        if (string.IsNullOrWhiteSpace(storageKey)) 
            throw new ArgumentException("Storage key cannot be empty", nameof(storageKey));
            
        Id = Guid.NewGuid(); // Генеруємо ID при створенні
        StorageKey = storageKey;
        MimeType = mimeType;
        Width = width;
        Height = height;
        AltText = altText;
    }

    // Метод для прив'язки до продукту (DDD стиль)
    public void AssignToProduct(Product product)
    {
        Product = product ?? throw new ArgumentNullException(nameof(product));
        ProductId = product.Id;
    }

    // Оновлення AltText (приклад бізнес-логіки)
    public void UpdateMetadata(string? altText)
    {
        AltText = altText;
    }
}