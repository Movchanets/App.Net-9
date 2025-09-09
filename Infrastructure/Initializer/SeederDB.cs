
using Infrastructure.Data.Constants;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Initializer;

public static class SeederDB
    {
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
                { Email = userEmail,
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
