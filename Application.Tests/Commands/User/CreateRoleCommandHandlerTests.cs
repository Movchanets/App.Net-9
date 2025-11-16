using Application.Commands.User;
using Application.Commands.User.CreateRole;
using Application.Interfaces;
using Application.ViewModels;
using FluentAssertions;
using Moq;

namespace Application.Tests.Commands.User;

/// <summary>
/// Тести для CreateRoleCommandHandler - обробника команди створення ролі
/// Перевіряють логіку створення ролей в системі
/// </summary>
public class CreateRoleCommandHandlerTests
{
    private readonly Mock<IUserService> _identityServiceMock;
    private readonly CreateRoleCommandHandler _handler;

    /// <summary>
    /// Конструктор тестового класу
    /// Ініціалізує mock-об'єкти та handler перед кожним тестом
    /// </summary>
    public CreateRoleCommandHandlerTests()
    {
        // Створюємо mock для IIdentityService - абстракція для роботи з Identity
        _identityServiceMock = new Mock<IUserService>();

        // Створюємо екземпляр handler'а з mock-залежністю
        _handler = new CreateRoleCommandHandler(_identityServiceMock.Object);
    }

    /// <summary>
    /// Тест 1: Перевірка успішного створення ролі
    /// Сценарій: Коли RoleManager успішно створює роль, handler повинен повернути успішну відповідь
    /// </summary>
    [Fact]
    public async Task Handle_WhenRoleCreationSucceeds_ShouldReturnSuccessResponse()
    {
        // Arrange (Підготовка) - налаштовуємо вхідні дані та поведінку mock-об'єктів
        var command = new CreateRoleCommand { RoleName = "Admin" };

        // Налаштовуємо mock: коли викликається CreateRoleAsync, повертаємо успішний результат
        _identityServiceMock
            .Setup(x => x.CreateRoleAsync("Admin"))
            .ReturnsAsync(true);

        // Act (Дія) - виконуємо метод, який тестуємо
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Перевірка) - перевіряємо, що результат відповідає очікуванням
        result.Should().NotBeNull(); // Результат не null
        result.IsSuccess.Should().BeTrue(); // Операція успішна
        result.Message.Should().Be("Role created successfully"); // Правильне повідомлення

        // Перевіряємо, що CreateRoleAsync був викликаний рівно 1 раз з роллю "Admin"
        _identityServiceMock.Verify(x => x.CreateRoleAsync("Admin"), Times.Once);
    }

    /// <summary>
    /// Тест 2: Перевірка невдалого створення ролі
    /// Сценарій: Коли RoleManager не може створити роль (наприклад, вона вже існує),
    /// handler повинен повернути помилку з детальним описом
    /// </summary>
    [Fact]
    public async Task Handle_WhenRoleCreationFails_ShouldReturnFailureResponse()
    {
        // Arrange - створюємо сценарій помилки
        var command = new CreateRoleCommand { RoleName = "Admin" };

        // Налаштовуємо mock: CreateRoleAsync повертає false (роль вже існує)
        _identityServiceMock
            .Setup(x => x.CreateRoleAsync("Admin"))
            .ReturnsAsync(false);

        // Act - виконуємо handler
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - перевіряємо обробку помилки
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse(); // Операція неуспішна
        result.Message.Should().Be("Role creation failed"); // Повідомлення про помилку
    }

    /// <summary>
    /// Тест 3: Перевірка обробки порожнього імені ролі
    /// Сценарій: Handler не валідує вхідні дані, він лише передає їх до RoleManager
    /// Це тест на те, що handler не блокує виклик навіть з порожнім ім'ям
    /// (валідація повинна бути на рівні FluentValidation або ModelState)
    /// </summary>
    [Fact]
    public async Task Handle_WhenRoleNameIsEmpty_ShouldStillCallCreateAsync()
    {
        // Arrange
        var command = new CreateRoleCommand { RoleName = string.Empty };
        _identityServiceMock
            .Setup(x => x.CreateRoleAsync(string.Empty))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - перевіряємо, що CreateRoleAsync був викликаний з порожнім ім'ям
        // Це показує, що handler не виконує валідацію (це правильно для CQRS)
        _identityServiceMock.Verify(x => x.CreateRoleAsync(string.Empty), Times.Once);
    }
}
