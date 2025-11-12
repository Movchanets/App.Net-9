using Application.Commands.User.Profile.ChangePassword;
using Application.ViewModels;
using FluentAssertions;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Threading;
using Xunit;

namespace Application.Tests.Commands.User;

public class ChangePasswordHandlerTests
{
	private readonly Mock<UserManager<UserEntity>> _userManagerMock;
	private readonly ChangePasswordHandler _handler;

	public ChangePasswordHandlerTests()
	{
		var userStore = new Mock<IUserStore<UserEntity>>();
		_userManagerMock = new Mock<UserManager<UserEntity>>(userStore.Object, null, null, null, null, null, null, null, null);
		_handler = new ChangePasswordHandler(_userManagerMock.Object);
	}

	[Fact]
	public async System.Threading.Tasks.Task Handle_WhenChangeSucceeds_ReturnsSuccess()
	{
		var user = new UserEntity { Id = 11, UserName = "joe" };
		_userManagerMock.Setup(x => x.FindByIdAsync("11")).ReturnsAsync(user);
		_userManagerMock.Setup(x => x.ChangePasswordAsync(user, "old", "new")).ReturnsAsync(IdentityResult.Success);

		var vm = new ChangePasswordVM { CurrentPassword = "old", NewPassword = "new" };
		var result = await _handler.Handle(new ChangePasswordCommand(11, vm), CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	public async System.Threading.Tasks.Task Handle_WhenChangeFails_ReturnsFailure()
	{
		var user = new UserEntity { Id = 12, UserName = "mary" };
		_userManagerMock.Setup(x => x.FindByIdAsync("12")).ReturnsAsync(user);
		var errors = new[] { new IdentityError { Description = "Wrong password" } };
		_userManagerMock.Setup(x => x.ChangePasswordAsync(user, "bad", "new")).ReturnsAsync(IdentityResult.Failed(errors));

		var vm = new ChangePasswordVM { CurrentPassword = "bad", NewPassword = "new" };
		var result = await _handler.Handle(new ChangePasswordCommand(12, vm), CancellationToken.None);

		result.IsSuccess.Should().BeFalse();
	}
}
