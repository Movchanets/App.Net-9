using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;

public class MediaImageConfiguration : IEntityTypeConfiguration<MediaImage>
{
	public void Configure(EntityTypeBuilder<MediaImage> builder)
	{
		builder.ToTable("MediaImages");
		builder.HasKey(m => m.Id);

		builder.Property(m => m.StorageKey)
			.IsRequired()
			.HasMaxLength(500);

		builder.Property(m => m.MimeType)
			.IsRequired()
			.HasMaxLength(100);

		builder.Property(m => m.Width)
			.IsRequired();

		builder.Property(m => m.Height)
			.IsRequired();

		builder.Property(m => m.AltText)
			.HasMaxLength(500);

		builder.HasIndex(m => m.StorageKey).IsUnique();
	}
}
