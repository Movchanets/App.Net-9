using Application.Commands.User.Profile.UpdateProfile;
using Application.ViewModels;
using FluentAssertions;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Threading;
using System.Collections.Generic;
using Xunit;

namespace Application.Tests.Commands.User;

public class UpdateProfileHandlerTests
{
	private readonly Mock<UserManager<UserEntity>> _userManagerMock;
	private readonly UpdateProfileHandler _handler;

	public UpdateProfileHandlerTests()
	{
		var userStore = new Mock<IUserStore<UserEntity>>();
		_userManagerMock = new Mock<UserManager<UserEntity>>(userStore.Object, null, null, null, null, null, null, null, null);
		_handler = new UpdateProfileHandler(_userManagerMock.Object);
	}

	[Fact]
	public async System.Threading.Tasks.Task Handle_WhenUpdateSucceeds_ReturnsUpdatedDto()
	{
		var user = new UserEntity { Id = 2, UserName = "old", Name = "Old", Surname = "User", Email = "old@example.com" };
		_userManagerMock.Setup(x => x.FindByIdAsync("2")).ReturnsAsync(user);
		_userManagerMock.Setup(x => x.FindByNameAsync("newname")).ReturnsAsync((UserEntity?)null);
		_userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<UserEntity>())).ReturnsAsync(IdentityResult.Success);
		_userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<UserEntity>())).ReturnsAsync(new List<string> { "User" });

		var vm = new UpdateProfileVM { Username = "newname", Name = "New" };
		var result = await _handler.Handle(new UpdateProfileCommand(2, vm), CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		result.Payload.Should().NotBeNull();
		result.Payload!.Username.Should().Be("newname");
	}

	[Fact]
	public async System.Threading.Tasks.Task Handle_WhenUsernameTaken_ReturnsFailure()
	{
		var user = new UserEntity { Id = 3, UserName = "abc" };
		var other = new UserEntity { Id = 4, UserName = "taken" };
		_userManagerMock.Setup(x => x.FindByIdAsync("3")).ReturnsAsync(user);
		_userManagerMock.Setup(x => x.FindByNameAsync("taken")).ReturnsAsync(other);

		var vm = new UpdateProfileVM { Username = "taken" };
		var result = await _handler.Handle(new UpdateProfileCommand(3, vm), CancellationToken.None);

		result.IsSuccess.Should().BeFalse();
		result.Message.Should().Be("Username is already taken");
	}
}
