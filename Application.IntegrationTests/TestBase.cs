using Infrastructure;
using Infrastructure.Entities.Identity;
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
    protected readonly UserManager<ApplicationUser> UserManager;
    protected readonly RoleManager<RoleEntity> RoleManager;
    protected readonly Application.Interfaces.IUserService IdentityService;
    protected readonly Domain.Interfaces.Repositories.IUserRepository UserRepository;
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
        // Для тестів замінимо провайдера токенів на простий детермінований провайдер,
        // щоб уникнути викликів DPAPI / нативних методів шифрування під час тестів.
        var identityBuilder = services.AddIdentity<ApplicationUser, RoleEntity>(options =>
            {
                // Ті ж налаштування, що й у Program.cs
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // Додаємо простий тестовий провайдер токенів під назвою "Simple" і використовуємо його
        // для скидання паролю у тестовому середовищі.
        identityBuilder.AddTokenProvider<SimpleTestTokenProvider<ApplicationUser>>(SimpleTestTokenProvider<ApplicationUser>.ProviderName);
        services.Configure<Microsoft.AspNetCore.Identity.IdentityOptions>(opts =>
        {
            opts.Tokens.PasswordResetTokenProvider = SimpleTestTokenProvider<ApplicationUser>.ProviderName;
        });

        // Реєструємо репозиторій та сервіси
        services.AddScoped<Domain.Interfaces.Repositories.IUserRepository, Infrastructure.Repositories.UserRepository>();
        services.AddScoped<Application.Interfaces.IUserService, Infrastructure.Services.UserService>();

        // Будуємо ServiceProvider (контейнер залежностей)
        _serviceProvider = services.BuildServiceProvider();

        // Отримуємо екземпляри сервісів
        DbContext = _serviceProvider.GetRequiredService<AppDbContext>();
        UserManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        RoleManager = _serviceProvider.GetRequiredService<RoleManager<RoleEntity>>();
        IdentityService = _serviceProvider.GetRequiredService<Application.Interfaces.IUserService>();
        UserRepository = _serviceProvider.GetRequiredService<Domain.Interfaces.Repositories.IUserRepository>();

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

    // Простий детермінований провайдер токенів для тестів.
    // Генерує токен на основі SecurityStamp користувача та purpose, і валідуює зворотно.
    public class SimpleTestTokenProvider<TUser> : Microsoft.AspNetCore.Identity.IUserTwoFactorTokenProvider<TUser>
        where TUser : class
    {
        public const string ProviderName = "Simple";

        public Task<string> GenerateAsync(string purpose, Microsoft.AspNetCore.Identity.UserManager<TUser> manager, TUser user)
        {
            // Використовуємо security stamp як основну секретну частину. Якщо його немає, використовуємо GUID.
            return Task.Run(async () =>
            {
                var stamp = await manager.GetSecurityStampAsync(user).ConfigureAwait(false);
                // Fallback: якщо stamp == null, беремо GUID
                if (string.IsNullOrEmpty(stamp)) stamp = Guid.NewGuid().ToString("N");
                var raw = stamp + ":" + purpose;
                return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(raw));
            });
        }

        public Task<bool> ValidateAsync(string purpose, string token, Microsoft.AspNetCore.Identity.UserManager<TUser> manager, TUser user)
        {
            return Task.Run(async () =>
            {
                var expected = await GenerateAsync(purpose, manager, user).ConfigureAwait(false);
                return expected == token;
            });
        }

        public Task<bool> CanGenerateTwoFactorTokenAsync(Microsoft.AspNetCore.Identity.UserManager<TUser> manager, TUser user)
        {
            return Task.FromResult(true);
        }
    }
}
