using System.Text.RegularExpressions;
using Application.Commands.User.CreateUser;
using Application.ViewModels;
using FluentAssertions;
using Infrastructure.Data.Constants;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Application.Tests.Commands.User;

/// <summary>
/// Тести для RegisterUserHandler - обробника команди реєстрації користувача
/// Перевіряють повний процес реєстрації: створення користувача, створення ролі, призначення ролі
/// </summary>
public class RegisterUserHandlerTests
{
    private readonly Mock<UserManager<UserEntity>> _userManagerMock;
    private readonly Mock<RoleManager<RoleEntity>> _roleManagerMock;
    private readonly RegisterUserHandler _handler;

    /// <summary>
    /// Конструктор - ініціалізує mock-об'єкти для UserManager та RoleManager
    /// </summary>
    public RegisterUserHandlerTests()
    {
        // Створюємо mock для UserStore (зберігання користувачів)
        var userStore = new Mock<IUserStore<UserEntity>>();
        // UserManager - клас ASP.NET Identity для управління користувачами (створення, паролі, ролі)
        _userManagerMock = new Mock<UserManager<UserEntity>>(
            userStore.Object, null, null, null, null, null, null, null, null);

        // Створюємо mock для RoleStore
        var roleStore = new Mock<IRoleStore<RoleEntity>>();
        _roleManagerMock = new Mock<RoleManager<RoleEntity>>(
            roleStore.Object, null, null, null, null);

        // Створюємо handler з обома mock-залежностями
        _handler = new RegisterUserHandler(_userManagerMock.Object, _roleManagerMock.Object);
    }

    /// <summary>
    /// Тест 1: Перевірка успішної реєстрації користувача
    /// Сценарій: Користувач створюється, роль вже існує, роль призначається - все успішно
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserCreationSucceeds_ShouldReturnSuccessResponse()
    {
        // Arrange - підготовка даних реєстрації
        var registrationData = new RegistrationVM
        {
            Name = "John",
            Surname = "Doe",
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };
        var command = new RegisterUserCommand(registrationData);

        // Налаштовуємо mock: створення користувача успішне
        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Налаштовуємо mock: роль "User" вже існує в системі
        _roleManagerMock
            .Setup(x => x.RoleExistsAsync(Roles.User))
            .ReturnsAsync(true);

        // Налаштовуємо mock: призначення ролі користувачу успішне
        _userManagerMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<UserEntity>(), Roles.User))
            .ReturnsAsync(IdentityResult.Success);

        // Act - виконуємо реєстрацію
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - перевіряємо результат
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("User created successfully");

        // Перевіряємо, що користувач був створений з правильними даними
        _userManagerMock.Verify(x => x.CreateAsync(
            It.Is<UserEntity>(u => Regex.IsMatch(u.UserName, ValidationRegexPattern.UsernameValidationPattern) && u.Email == "test@example.com"),
            "Password123!"), Times.Once);

        // Перевіряємо, що роль була призначена
        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<UserEntity>(), Roles.User), Times.Once);
    }

    /// <summary>
    /// Тест 2: Перевірка невдалої реєстрації (слабкий пароль)
    /// Сценарій: UserManager не приймає пароль - handler повертає помилку і не продовжує обробку
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserCreationFails_ShouldReturnFailureResponse()
    {
        // Arrange - дані з недопустимим паролем
        var registrationData = new RegistrationVM
        {
            Name = "John",
            Surname = "Doe",
            Email = "test@example.com",
            Password = "weak", // Слабкий пароль
            ConfirmPassword = "weak"
        };
        var command = new RegisterUserCommand(registrationData);

        // Створюємо помилку від Identity
        var errors = new[]
        {
            new IdentityError { Description = "Password too weak" }
        };

        // Налаштовуємо mock: CreateAsync повертає помилку
        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(errors));

        // Act - виконуємо реєстрацію
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - перевіряємо обробку помилки
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User creation failed");
        result.Payload.Should().BeOfType<List<string>>()
            .Which.Should().Contain("Password too weak");

        // Перевіряємо, що AddToRoleAsync НЕ викликався (бо користувач не був створений)
        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<UserEntity>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Тест 3: Перевірка автоматичного створення ролі
    /// Сценарій: Якщо роль не існує, handler автоматично створює її перед призначенням
    /// </summary>
    [Fact]
    public async Task Handle_WhenRoleDoesNotExist_ShouldCreateRole()
    {
        // Arrange
        var registrationData = new RegistrationVM
        {
            Name = "John",
            Surname = "Doe",
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };
        var command = new RegisterUserCommand(registrationData);

        // Користувач створюється успішно
        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Роль "User" ще НЕ існує в системі
        _roleManagerMock
            .Setup(x => x.RoleExistsAsync(Roles.User))
            .ReturnsAsync(false);

        // Створення ролі успішне
        _roleManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<RoleEntity>()))
            .ReturnsAsync(IdentityResult.Success);

        // Призначення ролі успішне
        _userManagerMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<UserEntity>(), Roles.User))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Перевіряємо, що handler перевірив існування ролі
        _roleManagerMock.Verify(x => x.RoleExistsAsync(Roles.User), Times.Once);
        // Перевіряємо, що роль була створена
        _roleManagerMock.Verify(x => x.CreateAsync(It.Is<RoleEntity>(r => r.Name == Roles.User)), Times.AtLeastOnce);
    }

    /// <summary>
    /// Тест 4: Перевірка коректного мапінгу даних
    /// Сценарій: Перевіряємо, що дані з RegistrationVM правильно переносяться в UserEntity
    /// Використовуємо Callback для "перехоплення" об'єкта, який передається в CreateAsync
    /// </summary>
    [Fact]
    public async Task Handle_ShouldMapRegistrationDataCorrectly()
    {
        // Arrange
        var registrationData = new RegistrationVM
        {
            Name = "John",
            Surname = "Doe",
            Email = "john@example.com",
            Password = "SecurePass123!",
            ConfirmPassword = "SecurePass123!"
        };
        var command = new RegisterUserCommand(registrationData);

        // Змінна для збереження об'єкта UserEntity, який передається в CreateAsync
        UserEntity capturedUser = null;

        // Callback - це техніка Moq, яка дозволяє "підслуховувати" параметри виклику
        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), It.IsAny<string>()))
            .Callback<UserEntity, string>((user, password) => capturedUser = user) // Зберігаємо user
            .ReturnsAsync(IdentityResult.Success);

        _roleManagerMock
            .Setup(x => x.RoleExistsAsync(Roles.User))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<UserEntity>(), Roles.User))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - перевіряємо правильність мапінгу
        capturedUser.Should().NotBeNull();
        capturedUser.Name.Should().Be("John");
        capturedUser.Surname.Should().Be("Doe");
        capturedUser.UserName.Should().MatchRegex(ValidationRegexPattern.UsernameValidationPattern);
        capturedUser.Email.Should().Be("john@example.com");
    }
}
