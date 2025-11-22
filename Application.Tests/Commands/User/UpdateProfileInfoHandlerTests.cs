using Application.Commands.User.Profile.UpdateProfileInfo;
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

public class UpdateProfileInfoHandlerTests
{
	private readonly Mock<IUserService> _identityServiceMock = new();
	private readonly Mock<ILogger<UpdateProfileInfoHandler>> _loggerMock = new();

	[Fact]
	public async System.Threading.Tasks.Task Handle_WhenUpdateSucceeds_ReturnsUpdatedDto()
	{
		var id = Guid.NewGuid();
		var dto = new UserDto(id, "newname", "New", "Surname", "e@mail.com", "+3800000000", new List<string> { "User" });
		_identityServiceMock
			.Setup(s => s.UpdateProfileInfoAsync(id, "newname", "New", "Surname"))
			.ReturnsAsync(dto);

		var handler = new UpdateProfileInfoHandler(_identityServiceMock.Object, _loggerMock.Object);
		var result = await handler.Handle(new UpdateProfileInfoCommand(id, new UpdateProfileInfoVM { Username = "newname", Name = "New", Surname = "Surname" }), CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		result.Payload!.Username.Should().Be("newname");
		result.Payload!.Name.Should().Be("New");
	}

	[Fact]
	public async System.Threading.Tasks.Task Handle_WhenUpdateFails_ReturnsFailure()
	{
		var id = Guid.NewGuid();
		_identityServiceMock
			.Setup(s => s.UpdateProfileInfoAsync(id, null, null, null))
			.ReturnsAsync((UserDto?)null);

		var handler = new UpdateProfileInfoHandler(_identityServiceMock.Object, _loggerMock.Object);
		var result = await handler.Handle(new UpdateProfileInfoCommand(id, new UpdateProfileInfoVM()), CancellationToken.None);

		result.IsSuccess.Should().BeFalse();
	}
}
