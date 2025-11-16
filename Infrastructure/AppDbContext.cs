using System;
using Domain.Entities;
using Infrastructure.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<
    ApplicationUser,
    RoleEntity,
    Guid,
    IdentityUserClaim<Guid>,
    ApplicationUserRole,
    IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>,
    IdentityUserToken<Guid>>(options)
{
    // Domain entities
    public DbSet<User> DomainUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Налаштування Domain User
        builder.Entity<User>(user =>
         {
             user.ToTable("DomainUsers");
             user.HasKey(u => u.Id);

             // --- КРИТИЧНЕ ВДОСКОНАЛЕННЯ ---
             // Явно вказуємо використовувати послідовний Guid для продуктивності
             user.Property(u => u.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>(); //

             user.Property(u => u.Name).HasMaxLength(100);
             user.Property(u => u.Surname).HasMaxLength(100);
             user.Property(u => u.Email).HasMaxLength(100);
             user.Property(u => u.PhoneNumber).HasMaxLength(20);
             user.Property(u => u.ImageUrl).HasMaxLength(500);
             user.Property(u => u.IsBlocked).IsRequired();

             user.HasIndex(u => u.IdentityUserId).IsUnique();
         });

        // Налаштування ApplicationUser (Identity)
        builder.Entity<ApplicationUser>(appUser =>
        {
            appUser.ToTable("AspNetUsers");

            // --- КРИТИЧНЕ ВДОСКОНАЛЕННЯ ---
            appUser.Property(au => au.Id)
               .ValueGeneratedOnAdd()
               .HasValueGenerator<SequentialGuidValueGenerator>(); //

            // Зв'язок ApplicationUser -> DomainUser (один до одного)
            appUser.HasOne(au => au.DomainUser)
               .WithOne()
               .HasForeignKey<ApplicationUser>(au => au.DomainUserId)
                    .IsRequired(false)
               .OnDelete(DeleteBehavior.Cascade);

            appUser.Property(au => au.RefreshToken).HasMaxLength(500);
        });

        // Налаштування ApplicationUserRole (many-to-many)
        builder.Entity<ApplicationUserRole>(userRole =>
        {
            userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

            userRole.HasOne(ur => ur.User)
               .WithMany(u => u.UserRoles)
               .HasForeignKey(ur => ur.UserId)
               .IsRequired();

            userRole.HasOne(ur => ur.Role)
               .WithMany(r => r.UserRoles)
               .HasForeignKey(ur => ur.RoleId)
               .IsRequired();
        });

        // Налаштування RoleEntity
        builder.Entity<RoleEntity>(role =>
        {
            // --- КРИТИЧНЕ ВДОСКОНАЛЕННЯ ---
            role.Property(r => r.Id)
               .ValueGeneratedOnAdd()
               .HasValueGenerator<SequentialGuidValueGenerator>(); //

            role.Property(r => r.Description).HasMaxLength(500);
        });
    }
}