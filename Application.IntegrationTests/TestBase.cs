using Infrastructure;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.IntegrationTests;

/// <summary>
/// Базовий клас для інтеграційних тестів
/// Налаштовує реальну in-memory БД та Identity систему
/// </summary>
public abstract class TestBase : IDisposable
{
    protected readonly AppDbContext DbContext;
    protected readonly UserManager<UserEntity> UserManager;
    protected readonly RoleManager<RoleEntity> RoleManager;
    private readonly ServiceProvider _serviceProvider;

    protected TestBase()
    {
        // Створюємо ServiceCollection для Dependency Injection
        var services = new ServiceCollection();

        // Додаємо логування (необхідне для Identity)
        services.AddLogging();

        // Налаштовуємо in-memory БД (кожен тест отримує окрему БД)
        var dbName = $"TestDb_{Guid.NewGuid()}"; // Унікальна БД для кожного тесту
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        // Налаштовуємо ASP.NET Core Identity (як у реальному додатку)
        services.AddIdentity<UserEntity, RoleEntity>(options =>
            {
                // Ті ж налаштування, що й у Program.cs
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // Будуємо ServiceProvider (контейнер залежностей)
        _serviceProvider = services.BuildServiceProvider();

        // Отримуємо екземпляри сервісів
        DbContext = _serviceProvider.GetRequiredService<AppDbContext>();
        UserManager = _serviceProvider.GetRequiredService<UserManager<UserEntity>>();
        RoleManager = _serviceProvider.GetRequiredService<RoleManager<RoleEntity>>();

        // Створюємо схему БД (in-memory БД потребує ініціалізації)
        DbContext.Database.EnsureCreated();
    }

    /// <summary>
    /// Очищення після тесту - видаляємо БД
    /// </summary>
    public void Dispose()
    {
        DbContext?.Database.EnsureDeleted();
        DbContext?.Dispose();
        _serviceProvider?.Dispose();
    }
}
