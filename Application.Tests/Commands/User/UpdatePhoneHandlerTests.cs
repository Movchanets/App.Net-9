using Application.Commands.User.Profile.UpdatePhone;
using Application.DTOs;
using Application.Interfaces;
using Application.ViewModels;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Application.Tests.Commands.User;

public class UpdatePhoneHandlerTests
{
	private readonly Mock<IUserService> _identityServiceMock = new();
	private readonly Mock<ILogger<UpdatePhoneHandler>> _loggerMock = new();

	[Fact]
	public async System.Threading.Tasks.Task Handle_WhenUpdateSucceeds_ReturnsUpdatedDto()
	{
		var id = Guid.NewGuid();
		var dto = new UserDto(id, "user", "Name", "Surname", "e@mail.com", "+3800000000", new List<string> { "User" });
		_identityServiceMock
			.Setup(s => s.UpdatePhoneAsync(id, "+3800000000"))
			.ReturnsAsync(dto);

		var handler = new UpdatePhoneHandler(_identityServiceMock.Object, _loggerMock.Object);
		var result = await handler.Handle(new UpdatePhoneCommand(id, new UpdatePhoneVM { PhoneNumber = "+3800000000" }), CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		result.Payload!.PhoneNumber.Should().Be("+3800000000");
	}

	[Fact]
	public async System.Threading.Tasks.Task Handle_WhenUpdateFails_ReturnsFailure()
	{
		var id = Guid.NewGuid();
		_identityServiceMock
			.Setup(s => s.UpdatePhoneAsync(id, "+380"))
			.ReturnsAsync((UserDto?)null);

		var handler = new UpdatePhoneHandler(_identityServiceMock.Object, _loggerMock.Object);
		var result = await handler.Handle(new UpdatePhoneCommand(id, new UpdatePhoneVM { PhoneNumber = "+380" }), CancellationToken.None);

		result.IsSuccess.Should().BeFalse();
	}
}
