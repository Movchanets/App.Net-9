using Application.Commands.User.Profile.ChangePassword;
using Application.Interfaces;
using Application.ViewModels;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using Xunit;
using System;

namespace Application.Tests.Commands.User;

public class ChangePasswordHandlerTests
{
	private readonly Mock<IUserService> _identityServiceMock;
	private readonly Mock<ILogger<ChangePasswordHandler>> _loggerMock;
	private readonly ChangePasswordHandler _handler;

	public ChangePasswordHandlerTests()
	{
		_identityServiceMock = new Mock<IUserService>();
		_loggerMock = new Mock<ILogger<ChangePasswordHandler>>();
		_handler = new ChangePasswordHandler(_identityServiceMock.Object, _loggerMock.Object);
	}

	[Fact]
	public async System.Threading.Tasks.Task Handle_WhenChangeSucceeds_ReturnsSuccess()
	{
		var id = Guid.NewGuid();
		_identityServiceMock.Setup(x => x.ChangePasswordAsync(id, "old", "new")).ReturnsAsync(true);

		var vm = new ChangePasswordVM { CurrentPassword = "old", NewPassword = "new" };
		var result = await _handler.Handle(new ChangePasswordCommand(id, vm), CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	public async System.Threading.Tasks.Task Handle_WhenChangeFails_ReturnsFailure()
	{
		var id = Guid.NewGuid();
		_identityServiceMock.Setup(x => x.ChangePasswordAsync(id, "bad", "new")).ReturnsAsync(false);

		var vm = new ChangePasswordVM { CurrentPassword = "bad", NewPassword = "new" };
		var result = await _handler.Handle(new ChangePasswordCommand(id, vm), CancellationToken.None);

		result.IsSuccess.Should().BeFalse();
	}
}
