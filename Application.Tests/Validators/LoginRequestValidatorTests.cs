using Application.DTOs;
using Application.Validators;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Validators;

public class LoginRequestValidatorTests
{
	private readonly LoginRequestValidator _validator = new LoginRequestValidator();

	[Fact]
	public void Validate_ValidLogin_NoErrors()
	{
		var req = new LoginRequest("user@example.com", "password123");
		var result = _validator.Validate(req);
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_MissingEmail_ReturnsError()
	{
		var req = new LoginRequest("", "password123");
		var result = _validator.Validate(req);
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(e => e.PropertyName == "Email");
	}

	[Fact]
	public void Validate_ShortPassword_ReturnsError()
	{
		var req = new LoginRequest("user@example.com", "123");
		var result = _validator.Validate(req);
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(e => e.PropertyName == "Password");
	}
}
