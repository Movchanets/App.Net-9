using Application.Queries.User.GetProfile;
using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;
using System.Threading;
using System.Collections.Generic;
using Xunit;
using System;

namespace Application.Tests.Commands.User;

public class GetProfileHandlerTests
{
	private readonly Mock<IUserService> _identityServiceMock;
	private readonly Mock<IUserRepository> _userRepositoryMock;
	private readonly Mock<IMapper> _mapperMock;
	private readonly GetProfileHandler _handler;

	public GetProfileHandlerTests()
	{
		_identityServiceMock = new Mock<IUserService>();
		_userRepositoryMock = new Mock<IUserRepository>();
		_mapperMock = new Mock<IMapper>();
		_handler = new GetProfileHandler(_identityServiceMock.Object, _userRepositoryMock.Object, _mapperMock.Object);
	}

	[Fact]
	public async System.Threading.Tasks.Task Handle_WhenUserExists_ReturnsProfile()
	{
		var id = Guid.NewGuid();
		var dto = new UserDto(id, "jdoe", "John", "Doe", "jdoe@example.com", string.Empty, new List<string> { "User" });
		_identityServiceMock.Setup(x => x.GetIdentityInfoByIdAsync(id)).ReturnsAsync(dto);

		var result = await _handler.Handle(new GetProfileQuery(id), CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		result.Payload.Should().NotBeNull();
		result.Payload!.Email.Should().Be("jdoe@example.com");
		result.Payload.Username.Should().Be("jdoe");
	}

	[Fact]
	public async System.Threading.Tasks.Task Handle_WhenUserNotFound_ReturnsFailure()
	{
		var missing = Guid.NewGuid();
		_identityServiceMock.Setup(x => x.GetIdentityInfoByIdAsync(missing)).ReturnsAsync((UserDto?)null);
		var result = await _handler.Handle(new GetProfileQuery(missing), CancellationToken.None);
		result.IsSuccess.Should().BeFalse();
	}
}
