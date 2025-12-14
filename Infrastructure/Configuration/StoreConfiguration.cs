using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
	public void Configure(EntityTypeBuilder<Store> builder)
	{
		builder.ToTable("Stores");
		builder.HasKey(s => s.Id);

		builder.Property(s => s.Name)
			.IsRequired()
			.HasMaxLength(200);

		builder.Property(s => s.Slug)
			.IsRequired()
			.HasMaxLength(200);

		builder.HasIndex(s => s.Slug).IsUnique();
		builder.HasIndex(s => s.UserId).IsUnique();

		builder.Property(s => s.Description)
			.HasMaxLength(2000);

		builder.Property(s => s.IsVerified)
			.IsRequired();

		builder.Property(s => s.IsSuspended)
			.IsRequired();

		builder.HasOne(s => s.User)
			.WithOne()
			.HasForeignKey<Store>(s => s.UserId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
