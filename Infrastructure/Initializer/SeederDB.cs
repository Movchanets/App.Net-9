using Infrastructure.Data.Constants;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Infrastructure.Initializer;

public static class SeederDB
{
    private static async Task AddClaimToRoleIfNotExists(RoleManager<RoleEntity> roleManager, RoleEntity role, string type, string value)
    {
        var existingClaims = await roleManager.GetClaimsAsync(role);
        if (!existingClaims.Any(c => c.Type == type && c.Value == value))
        {
            await roleManager.AddClaimAsync(role, new Claim(type, value));
        }
    }
    public static async Task SeedDataAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserEntity>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<RoleEntity>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // застосувати міграції
        await dbContext.Database.MigrateAsync();

        // створюємо ролі
        if (!roleManager.Roles.Any())
        {
            await roleManager.CreateAsync(new RoleEntity { Name = Roles.Admin });
            await roleManager.CreateAsync(new RoleEntity { Name = Roles.User });
            await roleManager.CreateAsync(new RoleEntity { Name = Roles.Seller });

            // додаємо claims до ролей (перевірка щоб не дублювати)
            var adminRole = await roleManager.FindByNameAsync(Roles.Admin);
            if (adminRole != null)
            {
                await AddClaimToRoleIfNotExists(roleManager, adminRole, "permission", "users.manage");
                await AddClaimToRoleIfNotExists(roleManager, adminRole, "permission", "users.read");
                await AddClaimToRoleIfNotExists(roleManager, adminRole, "permission", "users.update");
                await AddClaimToRoleIfNotExists(roleManager, adminRole, "permission", "users.delete");
                await AddClaimToRoleIfNotExists(roleManager, adminRole, "permission", "stores.verify");
                await AddClaimToRoleIfNotExists(roleManager, adminRole, "permission", "stores.read.all");
                await AddClaimToRoleIfNotExists(roleManager, adminRole, "permission", "stores.suspend");
                await AddClaimToRoleIfNotExists(roleManager, adminRole, "permission", "stores.delete");
                await AddClaimToRoleIfNotExists(roleManager, adminRole, "permission", "categories.manage");
                await AddClaimToRoleIfNotExists(roleManager, adminRole, "permission", "payouts.read.all");
                await AddClaimToRoleIfNotExists(roleManager, adminRole, "permission", "payouts.process");
            }
            var sellerRole = await roleManager.FindByNameAsync(Roles.Seller);
            if (sellerRole != null)
            {
                await AddClaimToRoleIfNotExists(roleManager, sellerRole, "permission", "products.create");
                await AddClaimToRoleIfNotExists(roleManager, sellerRole, "permission", "products.read.self");
                await AddClaimToRoleIfNotExists(roleManager, sellerRole, "permission", "products.update.self");
                await AddClaimToRoleIfNotExists(roleManager, sellerRole, "permission", "products.delete.self");
                await AddClaimToRoleIfNotExists(roleManager, sellerRole, "permission", "orders.read.self");
                await AddClaimToRoleIfNotExists(roleManager, sellerRole, "permission", "orders.update.status");
                await AddClaimToRoleIfNotExists(roleManager, sellerRole, "permission", "store.update.self");
                await AddClaimToRoleIfNotExists(roleManager, sellerRole, "permission", "store.read.self");
                await AddClaimToRoleIfNotExists(roleManager, sellerRole, "permission", "payouts.read.self");
                await AddClaimToRoleIfNotExists(roleManager, sellerRole, "permission", "payouts.request");
                //... додати всі інші дозволи Продавця
            }
            var userRole = await roleManager.FindByNameAsync(Roles.User);
            if (userRole != null)
            {
                await AddClaimToRoleIfNotExists(roleManager, userRole, "permission", "orders.create");
                await AddClaimToRoleIfNotExists(roleManager, userRole, "permission", "orders.read.self");
                await AddClaimToRoleIfNotExists(roleManager, userRole, "permission", "reviews.create");
                await AddClaimToRoleIfNotExists(roleManager, userRole, "permission", "reviews.update.self");
                await AddClaimToRoleIfNotExists(roleManager, userRole, "permission", "profile.read.self");
                await AddClaimToRoleIfNotExists(roleManager, userRole, "permission", "profile.update.self");
            }
        }

        // створюємо адміністратора
        if (!userManager.Users.Any())
        {
            var adminEmail = "admin@example.com";
            var admin = new UserEntity
            {
                Email = adminEmail,
                UserName = "admin",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Qwerty-1!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, Roles.Admin);
                await userManager.AddToRoleAsync(admin, Roles.User);
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Error: {error.Code} - {error.Description}");
                }
            }

            // створюємо звичайного користувача
            var userEmail = "user@example.com";
            var user = new UserEntity
            {
                Email = userEmail,
                UserName = "user",
                EmailConfirmed = true
            };

            result = await userManager.CreateAsync(user, "User123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, Roles.User);

            }
            else
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Error: {error.Code} - {error.Description}");
                }
            }
        }
        await dbContext.SaveChangesAsync();

    }
}
