using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
	public void Configure(EntityTypeBuilder<Tag> builder)
	{
		builder.ToTable("Tags");
		builder.HasKey(t => t.Id);

		builder.Property(t => t.Name)
			.IsRequired()
			.HasMaxLength(100);

		builder.Property(t => t.Slug)
			.IsRequired()
			.HasMaxLength(100);

		builder.HasIndex(t => t.Name).IsUnique();
		builder.HasIndex(t => t.Slug).IsUnique();

		builder.Property(t => t.Description)
			.HasMaxLength(500);

		builder.Metadata.FindNavigation(nameof(Tag.ProductTags))?
			.SetPropertyAccessMode(PropertyAccessMode.Field);

		builder.HasMany(t => t.ProductTags)
			.WithOne(pt => pt.Tag)
			.HasForeignKey(pt => pt.TagId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
