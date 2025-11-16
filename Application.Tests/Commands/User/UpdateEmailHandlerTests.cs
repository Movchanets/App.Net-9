using Application.Commands.User.Profile.UpdateEmail;
using Application.DTOs;
using Application.Interfaces;
using Application.ViewModels;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Application.Tests.Commands.User;

public class UpdateEmailHandlerTests
{
	private readonly Mock<IUserService> _identityServiceMock = new();

	[Fact]
	public async System.Threading.Tasks.Task Handle_WhenUpdateSucceeds_ReturnsUpdatedDto()
	{
		var id = Guid.NewGuid();
		var dto = new UserDto(id, "user", "Name", "Surname", "new@mail.com", "+3800000000", new List<string> { "User" });
		_identityServiceMock
			.Setup(s => s.UpdateEmailAsync(id, "new@mail.com"))
			.ReturnsAsync(dto);

		var handler = new UpdateEmailHandler(_identityServiceMock.Object);
		var result = await handler.Handle(new UpdateEmailCommand(id, new UpdateEmailVM { Email = "new@mail.com" }), CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		result.Payload!.Email.Should().Be("new@mail.com");
	}

	[Fact]
	public async System.Threading.Tasks.Task Handle_WhenUpdateFails_ReturnsFailure()
	{
		var id = Guid.NewGuid();
		_identityServiceMock
			.Setup(s => s.UpdateEmailAsync(id, "bad"))
			.ReturnsAsync((UserDto?)null);

		var handler = new UpdateEmailHandler(_identityServiceMock.Object);
		var result = await handler.Handle(new UpdateEmailCommand(id, new UpdateEmailVM { Email = "bad" }), CancellationToken.None);

		result.IsSuccess.Should().BeFalse();
	}
}
