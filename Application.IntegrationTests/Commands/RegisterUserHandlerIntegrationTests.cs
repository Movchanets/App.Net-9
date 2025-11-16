using Application.Commands.User.RegisterUser;
using Application.ViewModels;
using FluentAssertions;
using Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.IntegrationTests.Commands;

/// <summary>
/// Інтеграційні тести для RegisterUserHandler
/// Перевіряють РЕАЛЬНЕ створення користувачів, ролей та їх зв'язків в БД
/// </summary>
public class RegisterUserHandlerIntegrationTests : TestBase
{
    private readonly RegisterUserHandler _handler;

    public RegisterUserHandlerIntegrationTests()
    {
        // Handler з РЕАЛЬНИМ IdentityService
        _handler = new RegisterUserHandler(IdentityService);
    }

    [Fact]
    public async Task Handle_ShouldCreateUserInDatabase()
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

        // Перевіряємо, що користувача ще немає
        var userBefore = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        userBefore.Should().BeNull();

        // Act - РЕАЛЬНО створюємо користувача
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("User created successfully");

        // ✨ Перевіряємо БД
        var userInDb = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        userInDb.Should().NotBeNull();
        userInDb!.UserName.Should().MatchRegex(ValidationRegexPattern.UsernameValidationPattern);
        userInDb.Email.Should().Be("test@example.com");
        userInDb.NormalizedEmail.Should().Be("TEST@EXAMPLE.COM");

        // Перевіряємо, що пароль захешований (не в plain text!)
        userInDb.PasswordHash.Should().NotBeNullOrEmpty();
        userInDb.PasswordHash.Should().NotBe("Password123!"); // Не plain text
    }

    [Fact]
    public async Task Handle_ShouldAssignUserRole()
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

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Перевіряємо, що користувач має роль "User"
        var user = await UserManager.FindByEmailAsync("john@example.com");
        user.Should().NotBeNull();

        var roles = await UserManager.GetRolesAsync(user!);
        roles.Should().Contain(Roles.User);

        // Перевіряємо зв'язок у БД через таблицю UserRoles
        var userRoles = await DbContext.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .ToListAsync();
        userRoles.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldCreateRoleIfNotExists()
    {
        // Arrange - переконуємось, що ролі "User" немає
        var rolesBefore = await DbContext.Roles.ToListAsync();
        rolesBefore.Should().BeEmpty();

        var registrationData = new RegistrationVM
        {
            Name = "John",
            Surname = "Doe",
            Email = "first@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };
        var command = new RegisterUserCommand(registrationData);

        // Act - handler має автоматично створити роль
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Перевіряємо, що роль створена в БД
        var roleInDb = await DbContext.Roles.FirstOrDefaultAsync(r => r.Name == Roles.User);
        roleInDb.Should().NotBeNull();
        roleInDb!.Name.Should().Be(Roles.User);
    }

    [Fact]
    public async Task Handle_WithWeakPassword_ShouldNotCreateUser()
    {
        // Arrange - слабкий пароль (не відповідає вимогам Identity)
        var registrationData = new RegistrationVM
        {
            Name = "John",
            Surname = "Doe",
            Email = "weak@example.com",
            Password = "123", // Слабкий пароль
            ConfirmPassword = "123"
        };
        var command = new RegisterUserCommand(registrationData);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - має провалитись
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User creation failed");
        result.Payload.Should().NotBeNull(); // Містить помилки валідації

        // ✨ Перевіряємо БД - користувач НЕ створений
        var userInDb = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == "weak@example.com");
        userInDb.Should().BeNull();

        // Перевіряємо, що таблиця Users порожня
        var usersCount = await DbContext.Users.CountAsync();
        usersCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ShouldReturnError()
    {
        // Arrange - спочатку створюємо користувача
        var firstRegistration = new RegistrationVM
        {
            Name = "John",
            Surname = "Doe",
            Email = "duplicate@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };
        await _handler.Handle(new RegisterUserCommand(firstRegistration), CancellationToken.None);

        // Намагаємось створити другого з таким же email
        var secondRegistration = new RegistrationVM
        {
            Name = "John",
            Surname = "Doe",
            Email = "duplicate@example.com", // Дублікат!
            Password = "Password456!",
            ConfirmPassword = "Password456!"
        };
        var command = new RegisterUserCommand(secondRegistration);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - має провалитись (email має бути унікальним)
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User creation failed");

        // Перевіряємо БД - має бути тільки 1 користувач
        var usersCount = await DbContext.Users.CountAsync(u => u.Email == "duplicate@example.com");
        usersCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldHashPassword_NotStorePlainText()
    {
        // Arrange
        var password = "MySecretPassword123!";
        var registrationData = new RegistrationVM
        {
            Name = "John",
            Surname = "Doe",
            Email = "secure@example.com",
            Password = password,
            ConfirmPassword = password
        };
        var command = new RegisterUserCommand(registrationData);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - перевіряємо, що пароль захешований
        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == "secure@example.com");
        user.Should().NotBeNull();

        // Пароль НЕ має зберігатися в plain text
        user!.PasswordHash.Should().NotBe(password);
        user.PasswordHash.Should().NotBeNullOrEmpty();

        // Але UserManager може перевірити пароль
        var isPasswordValid = await UserManager.CheckPasswordAsync(user, password);
        isPasswordValid.Should().BeTrue(); // Identity правильно верифікує пароль
    }
}
