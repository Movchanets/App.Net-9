using Application.Commands.User;
using Application.Commands.User.CreateRole;
using Application.ViewModels;
using FluentAssertions;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Application.Tests.Commands.User;

/// <summary>
/// Тести для CreateRoleCommandHandler - обробника команди створення ролі
/// Перевіряють логіку створення ролей в системі
/// </summary>
public class CreateRoleCommandHandlerTests
{
    private readonly Mock<RoleManager<RoleEntity>> _roleManagerMock;
    private readonly CreateRoleCommandHandler _handler;

    /// <summary>
    /// Конструктор тестового класу
    /// Ініціалізує mock-об'єкти та handler перед кожним тестом
    /// </summary>
    public CreateRoleCommandHandlerTests()
    {
        // Створюємо mock для RoleStore (зберігання ролей)
        var roleStore = new Mock<IRoleStore<RoleEntity>>();
        
        // Створюємо mock для RoleManager з усіма необхідними залежностями
        // RoleManager - це клас з ASP.NET Identity для управління ролями
        _roleManagerMock = new Mock<RoleManager<RoleEntity>>(
            roleStore.Object, null, null, null, null);
        
        // Створюємо екземпляр handler'а з mock-залежністю
        _handler = new CreateRoleCommandHandler(_roleManagerMock.Object);
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
        
        // Налаштовуємо mock: коли викликається CreateAsync, повертаємо успішний результат
        _roleManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<RoleEntity>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act (Дія) - виконуємо метод, який тестуємо
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Перевірка) - перевіряємо, що результат відповідає очікуванням
        result.Should().NotBeNull(); // Результат не null
        result.IsSuccess.Should().BeTrue(); // Операція успішна
        result.Message.Should().Be("Role created successfully"); // Правильне повідомлення
        
        // Перевіряємо, що CreateAsync був викликаний рівно 1 раз з роллю "Admin"
        _roleManagerMock.Verify(x => x.CreateAsync(It.Is<RoleEntity>(r => r.Name == "Admin")), Times.Once);
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
        
        // Створюємо список помилок від Identity
        var errors = new[]
        {
            new IdentityError { Description = "Role already exists" }
        };
        
        // Налаштовуємо mock: CreateAsync повертає неуспішний результат з помилками
        _roleManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<RoleEntity>()))
            .ReturnsAsync(IdentityResult.Failed(errors));

        // Act - виконуємо handler
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - перевіряємо обробку помилки
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse(); // Операція неуспішна
        result.Message.Should().Be("Role creation failed"); // Повідомлення про помилку
        
        // Перевіряємо, що Payload містить список помилок з правильним описом
        result.Payload.Should().BeOfType<List<string>>()
            .Which.Should().Contain("Role already exists");
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
        _roleManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<RoleEntity>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - перевіряємо, що CreateAsync був викликаний з порожнім ім'ям
        // Це показує, що handler не виконує валідацію (це правильно для CQRS)
        _roleManagerMock.Verify(x => x.CreateAsync(It.Is<RoleEntity>(r => r.Name == string.Empty)), Times.Once);
    }
}
