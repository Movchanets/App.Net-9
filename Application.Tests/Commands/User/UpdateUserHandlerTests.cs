using Application.Commands.User.UpdateUser;
using Application.ViewModels;
using FluentAssertions;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Threading;
using Xunit;
using System.Collections.Generic;

namespace Application.Tests.Commands.User;

public class UpdateUserHandlerTests
{
	private readonly Mock<UserManager<UserEntity>> _userManagerMock;
	private readonly UpdateUserHandler _handler;

	public UpdateUserHandlerTests()
	{
		var userStore = new Mock<IUserStore<UserEntity>>();
		_userManagerMock = new Mock<UserManager<UserEntity>>(userStore.Object, null, null, null, null, null, null, null, null);
		_handler = new UpdateUserHandler(_userManagerMock.Object);
	}

	[Fact]
	public async Task Handle_WhenUserExists_UpdatesAndReturnsDto()
	{
		var user = new UserEntity { Id = 1, UserName = "old", Name = "Old", Surname = "User", Email = "old@example.com" };
		_userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
		_userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<UserEntity>())).ReturnsAsync(IdentityResult.Success);
		_userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<UserEntity>())).ReturnsAsync(new List<string> { "User" });

		var vm = new UpdateUserVM { Name = "New", Email = "new@example.com" };
		var cmd = new UpdateUserCommand(1, vm);

		var result = await _handler.Handle(cmd, CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		result.Payload.Should().NotBeNull();
		result.Payload!.Email.Should().Be("new@example.com");
		result.Payload.Username.Should().Be(user.UserName);

		_userManagerMock.Verify(x => x.UpdateAsync(It.Is<UserEntity>(u => u.Name == "New" && u.Email == "new@example.com")), Times.Once);
	}

	[Fact]
	public async Task Handle_WhenUserNotFound_ReturnsFailure()
	{
		_userManagerMock.Setup(x => x.FindByIdAsync("2")).ReturnsAsync((UserEntity?)null);
		var cmd = new UpdateUserCommand(2, new UpdateUserVM { Name = "X" });
		var result = await _handler.Handle(cmd, CancellationToken.None);
		result.IsSuccess.Should().BeFalse();
	}
}
