using Application.Commands.User;
using Application.Commands.User.CreateRole;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Application.IntegrationTests.Commands;

/// <summary>
/// Інтеграційні тести для CreateRoleCommandHandler
/// Використовують РЕАЛЬНУ in-memory БД та РЕАЛЬНИЙ RoleManager
/// Перевіряють, що ролі дійсно зберігаються в БД
/// </summary>
public class CreateRoleCommandHandlerIntegrationTests : TestBase
{
    private readonly CreateRoleCommandHandler _handler;

    public CreateRoleCommandHandlerIntegrationTests()
    {
        // Створюємо handler з РЕАЛЬНИМ RoleManager (не mock!)
        _handler = new CreateRoleCommandHandler(RoleManager);
    }

    [Fact]
    public async Task Handle_ShouldCreateRoleInDatabase()
    {
        // Arrange
        var command = new CreateRoleCommand { RoleName = "Admin" };

        // Перевіряємо, що ролі ще немає в БД
        var roleBeforeTest = await DbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        roleBeforeTest.Should().BeNull(); // Роль не існує

        // Act - РЕАЛЬНО створюємо роль через handler
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - перевіряємо результат
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Role created successfully");

        // ✨ ГОЛОВНА РІЗНИЦЯ: перевіряємо БД!
        var roleInDb = await DbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        roleInDb.Should().NotBeNull(); // Роль ДІЙСНО збережена в БД
        roleInDb!.Name.Should().Be("Admin");
        roleInDb.NormalizedName.Should().Be("ADMIN"); // Identity автоматично створює normalized name
    }

    [Fact]
    public async Task Handle_WhenRoleAlreadyExists_ShouldReturnError()
    {
        // Arrange - спочатку створюємо роль в БД
        await RoleManager.CreateAsync(new Infrastructure.Entities.RoleEntity { Name = "User" });
        
        // Перевіряємо, що роль існує
        var existingRole = await DbContext.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        existingRole.Should().NotBeNull();

        // Намагаємось створити ту ж роль знову
        var command = new CreateRoleCommand { RoleName = "User" };

        // Act - спроба створити дублікат
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - має повернути помилку
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Role creation failed");
        result.Payload.Should().NotBeNull();
        
        // Перевіряємо, що в БД все ще тільки одна роль "User"
        var rolesCount = await DbContext.Roles.CountAsync(r => r.Name == "User");
        rolesCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldCreateMultipleRoles()
    {
        // Arrange & Act - створюємо кілька ролей
        var adminCommand = new CreateRoleCommand { RoleName = "Admin" };
        var userCommand = new CreateRoleCommand { RoleName = "User" };
        var moderatorCommand = new CreateRoleCommand { RoleName = "Moderator" };

        await _handler.Handle(adminCommand, CancellationToken.None);
        await _handler.Handle(userCommand, CancellationToken.None);
        await _handler.Handle(moderatorCommand, CancellationToken.None);

        // Assert - перевіряємо БД
        var allRoles = await DbContext.Roles.ToListAsync();
        allRoles.Should().HaveCount(3);
        allRoles.Select(r => r.Name).Should().Contain(new[] { "Admin", "User", "Moderator" });
    }

    [Fact]
    public async Task Handle_WithInvalidRoleName_ShouldHandleValidationError()
    {
        // Arrange - порожнє ім'я (Identity має свою валідацію)
        var command = new CreateRoleCommand { RoleName = "" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Identity поверне помилку валідації
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Role creation failed");
        
        // Перевіряємо, що роль НЕ збережена в БД
        var rolesCount = await DbContext.Roles.CountAsync();
        rolesCount.Should().Be(0);
    }
}
