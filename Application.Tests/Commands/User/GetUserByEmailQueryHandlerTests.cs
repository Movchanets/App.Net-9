using Application.Commands.User.Queries.GetUserByEmail;
using FluentAssertions;
using Infrastructure.Data.Models;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Application.Tests.Commands.User;

/// <summary>
/// Тести для GetUserByEmailQueryHandler - обробника запиту пошуку користувача за email
/// Перевіряють логіку пошуку та повернення даних користувача з ролями
/// </summary>
public class GetUserByEmailQueryHandlerTests
{
    private readonly Mock<UserManager<UserEntity>> _userManagerMock;
    private readonly GetUserByEmailQueryHandler _handler;

    /// <summary>
    /// Конструктор - ініціалізує mock для UserManager
    /// </summary>
    public GetUserByEmailQueryHandlerTests()
    {
        var userStore = new Mock<IUserStore<UserEntity>>();
        _userManagerMock = new Mock<UserManager<UserEntity>>(
            userStore.Object, null, null, null, null, null, null, null, null);

        _handler = new GetUserByEmailQueryHandler(_userManagerMock.Object);
    }

    /// <summary>
    /// Тест 1: Перевірка успішного пошуку користувача
    /// Сценарій: Користувач знайдений, повертаються його дані з ролями
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserExists_ShouldReturnSuccessResponseWithUserData()
    {
        // Arrange - підготовка тестових даних
        var email = "test@example.com";
        var query = new GetUserByEmailQuery(email);

        // Створюємо тестового користувача
        var user = new UserEntity
        {
            Id = 1,
            UserName = "testuser",
            Email = email
        };

        // Список ролей користувача
        var roles = new List<string> { "User", "Admin" };

        // Налаштовуємо mock: пошук за email повертає користувача
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        // Налаштовуємо mock: отримання ролей користувача
        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        // Act - виконуємо запит
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert - перевіряємо результат
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("User retrieved successfully");
        result.Payload.Should().BeOfType<UserVM>(); // Payload містить UserVM

        // Перевіряємо дані в UserVM
        var userVM = result.Payload as UserVM;
        userVM.Should().NotBeNull();
        userVM.Id.Should().Be(1);
        userVM.UserName.Should().Be("testuser");
        userVM.Email.Should().Be(email);
        userVM.UserRoles.Should().HaveCount(2); // 2 ролі
        userVM.UserRoles.Should().Contain(new[] { "User", "Admin" });

        // Перевіряємо, що методи були викликані правильно
        _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
        _userManagerMock.Verify(x => x.GetRolesAsync(user), Times.Once);
    }

    /// <summary>
    /// Тест 2: Перевірка пошуку неіснуючого користувача
    /// Сценарій: Користувач з таким email не знайдений - повертається помилка
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldReturnFailureResponse()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var query = new GetUserByEmailQuery(email);

        // Налаштовуємо mock: пошук повертає null (користувач не знайдений)
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((UserEntity)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse(); // Операція неуспішна
        result.Message.Should().Be("User not found");
        result.Payload.Should().BeNull(); // Немає даних

        _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
        // GetRolesAsync НЕ повинен викликатися (бо користувач не знайдений)
        _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<UserEntity>()), Times.Never);
    }

    /// <summary>
    /// Тест 3: Перевірка користувача без ролей
    /// Сценарій: Користувач існує, але не має жодних ролей - повертається успіх з порожнім списком ролей
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserExistsWithNoRoles_ShouldReturnSuccessResponseWithEmptyRoles()
    {
        // Arrange
        var email = "user@example.com";
        var query = new GetUserByEmailQuery(email);

        var user = new UserEntity
        {
            Id = 2,
            UserName = "basicuser",
            Email = email
        };

        // Користувач знайдений
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        // Але ролі порожні (новий користувач без призначення ролей)
        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue(); // Операція успішна
        
        var userVM = result.Payload as UserVM;
        userVM.Should().NotBeNull();
        userVM.UserRoles.Should().BeEmpty(); // Список ролей порожній
    }

    /// <summary>
    /// Тест 4: Перевірка обробки порожнього email
    /// Сценарій: Handler не валідує вхідні дані, він все одно спробує знайти користувача
    /// (валідація повинна бути на рівні FluentValidation або контролера)
    /// </summary>
    [Fact]
    public async Task Handle_WhenEmailIsEmpty_ShouldStillAttemptToFindUser()
    {
        // Arrange
        var query = new GetUserByEmailQuery(string.Empty);

        // Пошук з порожнім email поверне null
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(string.Empty))
            .ReturnsAsync((UserEntity)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        // Перевіряємо, що пошук все одно був викликаний (не було early return)
        _userManagerMock.Verify(x => x.FindByEmailAsync(string.Empty), Times.Once);
    }
}
