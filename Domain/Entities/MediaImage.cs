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

    // Оновлення AltText (приклад бізнес-логіки)
    public void UpdateMetadata(string? altText)
    {
        AltText = altText;
    }
}