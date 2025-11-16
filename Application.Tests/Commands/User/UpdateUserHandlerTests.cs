using Application.Commands.User.UpdateUser;
using Application.DTOs;
using Application.Interfaces;
using Application.ViewModels;
using FluentAssertions;
using Moq;
using System.Threading;
using Xunit;
using System.Collections.Generic;
using System;

namespace Application.Tests.Commands.User;

public class UpdateUserHandlerTests
{
	private readonly Mock<IUserService> _identityServiceMock;
	private readonly UpdateUserHandler _handler;

	public UpdateUserHandlerTests()
	{
		_identityServiceMock = new Mock<IUserService>();
		_handler = new UpdateUserHandler(_identityServiceMock.Object);
	}

	[Fact]
	public async Task Handle_WhenUserExists_UpdatesAndReturnsDto()
	{
		var id = Guid.NewGuid();
		var updatedInfo = new UserDto(id, "old", "New", string.Empty, "new@example.com", string.Empty, new List<string> { "User" });
		_identityServiceMock.Setup(x => x.UpdateIdentityProfileAsync(id, null, "new@example.com", null, "New", null))
			.ReturnsAsync(updatedInfo);

		var vm = new UpdateUserVM { Name = "New", Email = "new@example.com" };
		var cmd = new UpdateUserCommand(id, vm);

		var result = await _handler.Handle(cmd, CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		result.Payload.Should().NotBeNull();
		result.Payload!.Email.Should().Be("new@example.com");
		result.Payload.Username.Should().Be("old");

		_identityServiceMock.Verify(x => x.UpdateIdentityProfileAsync(id, null, "new@example.com", null, "New", null), Times.Once);
	}

	[Fact]
	public async Task Handle_WhenUserNotFound_ReturnsFailure()
	{
		var id = Guid.NewGuid();
		_identityServiceMock.Setup(x => x.UpdateIdentityProfileAsync(id, null, null, null, "X", null))
			.ReturnsAsync((UserDto?)null);
		var cmd = new UpdateUserCommand(id, new UpdateUserVM { Name = "X" });
		var result = await _handler.Handle(cmd, CancellationToken.None);
		result.IsSuccess.Should().BeFalse();
	}
}
