using Application.Interfaces;
using Application.ViewModels;
using Domain.Constants;
using FluentAssertions;
using Moq;
using System;
using Application.Commands.User.RegisterUser;

namespace Application.Tests.Commands.User;

/// <summary>
/// Тести для RegisterUserHandler - обробника команди реєстрації користувача
/// Перевіряють повний процес реєстрації: створення користувача, створення ролі, призначення ролі
/// </summary>
public class RegisterUserHandlerTests
{
    private readonly Mock<IUserService> _identityServiceMock;
    private readonly RegisterUserHandler _handler;

    /// <summary>
    /// Конструктор - ініціалізує mock для IIdentityService
    /// </summary>
    public RegisterUserHandlerTests()
    {
        _identityServiceMock = new Mock<IUserService>();
        _handler = new RegisterUserHandler(_identityServiceMock.Object);
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

        // Налаштовуємо mock: реєстрація успішна
        var newId = Guid.NewGuid();
        
        _identityServiceMock
            .Setup(x => x.RegisterAsync(registrationData))
            .ReturnsAsync((true, new List<string>(), newId));

        _identityServiceMock
            .Setup(x => x.EnsureRoleExistsAsync(Roles.User))
            .ReturnsAsync(true);

        _identityServiceMock
            .Setup(x => x.AddUserToRoleAsync(newId, Roles.User))
            .ReturnsAsync(true);

        // Act - виконуємо реєстрацію
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - перевіряємо результат
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("User created successfully");

        _identityServiceMock.Verify(x => x.RegisterAsync(registrationData), Times.Once);
        _identityServiceMock.Verify(x => x.AddUserToRoleAsync(newId, Roles.User), Times.Once);
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

        // Налаштовуємо mock: RegisterAsync повертає помилку
        _identityServiceMock
            .Setup(x => x.RegisterAsync(registrationData))
            .ReturnsAsync((false, new List<string> { "Password too weak" }, null));

        // Act - виконуємо реєстрацію
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - перевіряємо обробку помилки
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User creation failed");
        result.Payload.Should().BeOfType<List<string>>()
            .Which.Should().Contain("Password too weak");

        // Перевіряємо, що AddToRoleAsync НЕ викликався (бо користувач не був створений)
        _identityServiceMock.Verify(x => x.AddUserToRoleAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
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

        // Реєстрація успішна
        var createdId = Guid.NewGuid();
        _identityServiceMock
            .Setup(x => x.RegisterAsync(registrationData))
            .ReturnsAsync((true, new List<string>(), createdId));

        // EnsureRoleExistsAsync створить роль, якщо її немає
        _identityServiceMock
            .Setup(x => x.EnsureRoleExistsAsync(Roles.User))
            .ReturnsAsync(true);

        _identityServiceMock
            .Setup(x => x.AddUserToRoleAsync(createdId, Roles.User))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _identityServiceMock.Verify(x => x.EnsureRoleExistsAsync(Roles.User), Times.Once);
    }


}
