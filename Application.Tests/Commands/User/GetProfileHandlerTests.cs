using Application.Queries.User.GetProfile;
using Application.DTOs;
using FluentAssertions;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Threading;
using System.Collections.Generic;
using Xunit;

namespace Application.Tests.Commands.User;

public class GetProfileHandlerTests
{
	private readonly Mock<UserManager<UserEntity>> _userManagerMock;
	private readonly GetProfileHandler _handler;

	public GetProfileHandlerTests()
	{
		var userStore = new Mock<IUserStore<UserEntity>>();
		_userManagerMock = new Mock<UserManager<UserEntity>>(userStore.Object, null, null, null, null, null, null, null, null);
		_handler = new GetProfileHandler(_userManagerMock.Object);
	}

	[Fact]
	public async System.Threading.Tasks.Task Handle_WhenUserExists_ReturnsProfile()
	{
		var user = new UserEntity { Id = 7, UserName = "jdoe", Name = "John", Surname = "Doe", Email = "jdoe@example.com" };
		_userManagerMock.Setup(x => x.FindByIdAsync("7")).ReturnsAsync(user);
		_userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

		var result = await _handler.Handle(new GetProfileQuery(7), CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		result.Payload.Should().NotBeNull();
		result.Payload!.Email.Should().Be("jdoe@example.com");
		result.Payload.Username.Should().Be("jdoe");
	}

	[Fact]
	public async System.Threading.Tasks.Task Handle_WhenUserNotFound_ReturnsFailure()
	{
		_userManagerMock.Setup(x => x.FindByIdAsync("9")).ReturnsAsync((UserEntity?)null);
		var result = await _handler.Handle(new GetProfileQuery(9), CancellationToken.None);
		result.IsSuccess.Should().BeFalse();
	}
}
