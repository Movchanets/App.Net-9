using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // 1. Таблиця та Ключ
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        // 2. Властивості
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200); // Обмеження довжини для БД

        builder.Property(p => p.Description)
            .HasMaxLength(2000); // Опис може бути довгим, але не безкінечним

        // 3. Налаштування грошей (Критично для PostgreSQL!)
        // Postgres тип `numeric(18,2)` гарантує точність до копійок
        builder.Property(p => p.Price)
            .HasPrecision(18, 2) 
            .IsRequired();

        // 4. Унікальність SKU
        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(50);

        // Створюємо індекс, щоб пошук по артикулу літав, 
        // і забороняємо дублікати артикулів
        builder.HasIndex(p => p.Sku)
            .IsUnique();

        // 5. Зв'язок з MediaImage (One-to-Many)
        // Тут ми вказуємо, що при видаленні Product, видаляються і його Images
        builder.Metadata.FindNavigation(nameof(Product.Images))?
            .SetPropertyAccessMode(PropertyAccessMode.Field); // EF Core буде писати прямо в поле _images
            
        builder.HasMany(p => p.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}