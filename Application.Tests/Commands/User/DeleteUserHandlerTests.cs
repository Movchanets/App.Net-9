using Application.Commands.User.DeleteUser;
using FluentAssertions;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Threading;
using Xunit;

namespace Application.Tests.Commands.User;

public class DeleteUserHandlerTests
{
	private readonly Mock<UserManager<UserEntity>> _userManagerMock;
	private readonly DeleteUserHandler _handler;

	public DeleteUserHandlerTests()
	{
		var userStore = new Mock<IUserStore<UserEntity>>();
		_userManagerMock = new Mock<UserManager<UserEntity>>(userStore.Object, null, null, null, null, null, null, null, null);
		_handler = new DeleteUserHandler(_userManagerMock.Object);
	}

	[Fact]
	public async Task Handle_WhenUserExists_DeletesAndReturnsSuccess()
	{
		var user = new UserEntity { Id = 5, UserName = "todelete" };
		_userManagerMock.Setup(x => x.FindByIdAsync("5")).ReturnsAsync(user);
		_userManagerMock.Setup(x => x.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

		var res = await _handler.Handle(new DeleteUserCommand(5), CancellationToken.None);
		res.IsSuccess.Should().BeTrue();
	}

	[Fact]
	public async Task Handle_WhenUserNotFound_ReturnsFailure()
	{
		_userManagerMock.Setup(x => x.FindByIdAsync("10")).ReturnsAsync((UserEntity?)null);
		var res = await _handler.Handle(new DeleteUserCommand(10), CancellationToken.None);
		res.IsSuccess.Should().BeFalse();
	}
}
