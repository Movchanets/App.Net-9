using Application.Queries.User.GetUserByEmail;
using Application.Interfaces;
using Application.DTOs;
using FluentAssertions;
using Application.Models;
using Moq;
using System;

namespace Application.Tests.Commands.User;

/// <summary>
/// Тести для GetUserByEmailQueryHandler - обробника запиту пошуку користувача за email
/// Перевіряють логіку пошуку та повернення даних користувача з ролями
/// </summary>
public class GetUserByEmailQueryHandlerTests
{
    private readonly Mock<IUserService> _identityServiceMock;
    private readonly GetUserByEmailQueryHandler _handler;

    /// <summary>
    /// Конструктор - ініціалізує mock для IIdentityService
    /// </summary>
    public GetUserByEmailQueryHandlerTests()
    {
        _identityServiceMock = new Mock<IUserService>();
        _handler = new GetUserByEmailQueryHandler(_identityServiceMock.Object);
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

        // Створюємо DTO з даними користувача
        var identityInfo = new UserDto(
            Guid.NewGuid(),
            "testuser",
            string.Empty,
            string.Empty,
            email,
            string.Empty,
            new List<string> { "User", "Admin" }
        );

        // Налаштовуємо mock: пошук за email повертає IdentityUserInfoDto
        _identityServiceMock
            .Setup(x => x.GetIdentityInfoByEmailAsync(email))
            .ReturnsAsync(identityInfo);

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
        userVM.Id.Should().NotBe(Guid.Empty);
        userVM.UserName.Should().Be("testuser");
        userVM.Email.Should().Be(email);
        userVM.UserRoles.Should().HaveCount(2); // 2 ролі
        userVM.UserRoles.Should().Contain(new[] { "User", "Admin" });

        // Перевіряємо, що метод був викликаний правильно
        _identityServiceMock.Verify(x => x.GetIdentityInfoByEmailAsync(email), Times.Once);
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
        _identityServiceMock
            .Setup(x => x.GetIdentityInfoByEmailAsync(email))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse(); // Операція неуспішна
        result.Message.Should().Be("User not found");
        result.Payload.Should().BeNull(); // Немає даних

        _identityServiceMock.Verify(x => x.GetIdentityInfoByEmailAsync(email), Times.Once);
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

        var identityInfo = new UserDto(
            Guid.NewGuid(),
            "basicuser",
            string.Empty,
            string.Empty,
            email,
            string.Empty,
            new List<string>()
        );

        // Користувач знайдений, але ролі порожні
        _identityServiceMock
            .Setup(x => x.GetIdentityInfoByEmailAsync(email))
            .ReturnsAsync(identityInfo);

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
        _identityServiceMock
            .Setup(x => x.GetIdentityInfoByEmailAsync(string.Empty))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        // Перевіряємо, що пошук все одно був викликаний (не було early return)
        _identityServiceMock.Verify(x => x.GetIdentityInfoByEmailAsync(string.Empty), Times.Once);
    }
}
