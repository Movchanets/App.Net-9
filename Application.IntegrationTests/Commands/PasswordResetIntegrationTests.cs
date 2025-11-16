using System.Threading.Tasks;

using FluentAssertions;
using Infrastructure.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace Infrastructure.IntegrationTests.Commands;

public class PasswordResetIntegrationTests : TestBase
{
    [Fact]
    public async Task ResetPassword_Succeeds_WithValidToken()
    {
        var user = new ApplicationUser { UserName = "testuser", Email = "test@example.com" };
        var create = await UserManager.CreateAsync(user, "P@ssw0rd1");
        create.Succeeded.Should().BeTrue();

        var token = await UserManager.GeneratePasswordResetTokenAsync(user);
        token.Should().NotBeNullOrWhiteSpace();

        var reset = await UserManager.ResetPasswordAsync(user, token, "NewP@ss2");
        reset.Succeeded.Should().BeTrue();

        var check = await UserManager.CheckPasswordAsync(user, "NewP@ss2");
        check.Should().BeTrue();
    }

    [Fact]
    public async Task ResetPassword_Fails_WithInvalidToken()
    {
        var user = new ApplicationUser { UserName = "testuser2", Email = "test2@example.com" };
        var create = await UserManager.CreateAsync(user, "P@ssw0rd1");
        create.Succeeded.Should().BeTrue();

        var reset = await UserManager.ResetPasswordAsync(user, "invalid-token", "NewP@ss2");
        reset.Succeeded.Should().BeFalse();
    }
}
