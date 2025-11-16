using Application.Commands.User.DeleteUser;
using Application.Interfaces;
using FluentAssertions;
using Moq;
using System.Threading;
using Xunit;
using System;

namespace Application.Tests.Commands.User;

public class DeleteUserHandlerTests
{
	private readonly Mock<IUserService> _identityServiceMock;
	private readonly DeleteUserHandler _handler;

	public DeleteUserHandlerTests()
	{
		_identityServiceMock = new Mock<IUserService>();
		_handler = new DeleteUserHandler(_identityServiceMock.Object);
	}

	[Fact]
	public async Task Handle_WhenUserExists_DeletesAndReturnsSuccess()
	{
		var id = Guid.NewGuid();
		_identityServiceMock.Setup(x => x.DeleteUserByIdAsync(id)).ReturnsAsync(true);

		var res = await _handler.Handle(new DeleteUserCommand(id), CancellationToken.None);
		res.IsSuccess.Should().BeTrue();
	}

	[Fact]
	public async Task Handle_WhenUserNotFound_ReturnsFailure()
	{
		var id = Guid.NewGuid();
		_identityServiceMock.Setup(x => x.DeleteUserByIdAsync(id)).ReturnsAsync(false);
		var res = await _handler.Handle(new DeleteUserCommand(id), CancellationToken.None);
		res.IsSuccess.Should().BeFalse();
	}
}
