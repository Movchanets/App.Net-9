using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;

public class ProductGalleryConfiguration : IEntityTypeConfiguration<ProductGallery>
{
	public void Configure(EntityTypeBuilder<ProductGallery> builder)
	{
		builder.ToTable("ProductGalleries");
		builder.HasKey(pg => pg.Id);

		builder.Property(pg => pg.DisplayOrder)
			.IsRequired();

		builder.HasIndex(pg => new { pg.ProductId, pg.DisplayOrder });

		builder.HasOne(pg => pg.Product)
			.WithMany(p => p.Gallery)
			.HasForeignKey(pg => pg.ProductId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(pg => pg.MediaImage)
			.WithMany()
			.HasForeignKey(pg => pg.MediaImageId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
