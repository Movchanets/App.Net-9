using Application.Commands.User.Profile.UpdateProfile;
using Application.DTOs;
using Application.Interfaces;
using Application.ViewModels;
using FluentAssertions;
using Moq;
using System.Threading;
using System.Collections.Generic;
using Xunit;
using System;

namespace Application.Tests.Commands.User;

public class UpdateProfileHandlerTests
{
	private readonly Mock<IUserService> _identityServiceMock;
	private readonly UpdateProfileHandler _handler;

	public UpdateProfileHandlerTests()
	{
		_identityServiceMock = new Mock<IUserService>();
		_handler = new UpdateProfileHandler(_identityServiceMock.Object);
	}

	[Fact]
	public async System.Threading.Tasks.Task Handle_WhenUpdateSucceeds_ReturnsUpdatedDto()
	{
		var id = Guid.NewGuid();
		var updatedInfo = new UserDto(id, "newname", "New", string.Empty, "old@example.com", string.Empty, new List<string> { "User" });
		_identityServiceMock.Setup(x => x.UpdateIdentityProfileAsync(id, "newname", "New", null))
			.ReturnsAsync(updatedInfo);

		var vm = new UpdateProfileVM { Username = "newname", Name = "New" };
		var result = await _handler.Handle(new UpdateProfileCommand(id, vm), CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		result.Payload.Should().NotBeNull();
		result.Payload!.Username.Should().Be("newname");
	}

	[Fact]
	public async System.Threading.Tasks.Task Handle_WhenUserNotFound_ReturnsFailure()
	{
		var id = Guid.NewGuid();
		_identityServiceMock.Setup(x => x.UpdateIdentityProfileAsync(id, "newname", null, null))
			.ReturnsAsync((UserDto?)null);

		var vm = new UpdateProfileVM { Username = "newname" };
		var result = await _handler.Handle(new UpdateProfileCommand(id, vm), CancellationToken.None);

		result.IsSuccess.Should().BeFalse();
		result.Message.Should().Be("Profile update failed");
	}
}
