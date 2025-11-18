namespace Domain.Entities;

public class Product : BaseEntity<Guid>
{
 
   
    public string Name { get; private set; }
    public string? Description { get; private set; }
    
    // Артикул (SKU) - унікальний ідентифікатор товару для складу
    public string Sku { get; private set; }

    // Ціна (використовуємо decimal для грошей!)
    public decimal Price { get; private set; }
    
    // Кількість на складі
    public int StockQuantity { get; private set; }
    // Колекція картинок
    private readonly List<MediaImage> _images = new();
    public virtual IReadOnlyCollection<MediaImage> Images => _images.AsReadOnly();
    // Конструктор для EF Core
    private Product() { }
    // Публічний конструктор
    public Product(string name, string sku, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentNullException(nameof(sku));
        if (price < 0) throw new ArgumentException("Price cannot be negative");

        Id = Guid.NewGuid();
        Name = name;
        Sku = sku;
        Price = price;
    }
    // Методи бізнес-логіки
    public void UpdateStock(int quantity)
    {
        if (quantity < 0) throw new ArgumentException("Stock cannot be negative");
        StockQuantity = quantity;
    }
    // Метод додавання картинки
    public void AddImage(MediaImage image)
    {
        image.AssignToProduct(this); // Встановлюємо зв'язок
        _images.Add(image);
    }
}