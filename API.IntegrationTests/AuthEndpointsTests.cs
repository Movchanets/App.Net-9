using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using System.Threading.Tasks;

namespace API.IntegrationTests;

public class AuthEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
	private readonly HttpClient _client;

	public AuthEndpointsTests(TestWebApplicationFactory factory)
	{
		_client = factory.CreateClient();
	}

	[Fact]
	public async Task Login_Admin_ReturnsTokenPair()
	{
		var payload = new { email = "admin@example.com", password = "Qwerty-1!" };
		var resp = await _client.PostAsJsonAsync("/api/auth/login", payload);
		resp.StatusCode.Should().Be(HttpStatusCode.OK);

		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		json.TryGetProperty("accessToken", out var accessToken).Should().BeTrue();
		json.TryGetProperty("refreshToken", out var refreshToken).Should().BeTrue();
		accessToken.GetString().Should().NotBeNullOrWhiteSpace();
		refreshToken.GetString().Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task Login_InvalidPassword_Returns401()
	{
		var payload = new { email = "admin@example.com", password = "wrong" };
		var resp = await _client.PostAsJsonAsync("/api/auth/login", payload);
		resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		// Response JSON uses default camelCase policy
		json.TryGetProperty("message", out var message).Should().BeTrue();
		message.GetString().Should().Be("Invalid credentials");
	}

	[Fact]
	public async Task RefreshToken_Valid_ReturnsNewTokens()
	{
		var login = new { email = "admin@example.com", password = "Qwerty-1!" };
		var loginResp = await _client.PostAsJsonAsync("/api/auth/login", login);
		loginResp.EnsureSuccessStatusCode();
		var loginJson = await loginResp.Content.ReadFromJsonAsync<JsonElement>();
		var access = loginJson.GetProperty("accessToken").GetString();
		var refresh = loginJson.GetProperty("refreshToken").GetString();

		var refreshReq = new { accessToken = access, refreshToken = refresh };
		var resp = await _client.PostAsJsonAsync("/api/auth/refresh", refreshReq);
		resp.StatusCode.Should().Be(HttpStatusCode.OK);

		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		var newAccess = json.GetProperty("accessToken").GetString();
		var newRefresh = json.GetProperty("refreshToken").GetString();
		newAccess.Should().NotBeNullOrWhiteSpace();
		newRefresh.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task RefreshToken_Invalid_Returns401()
	{
		var refreshReq = new { accessToken = "", refreshToken = "invalid" };
		var resp = await _client.PostAsJsonAsync("/api/auth/refresh", refreshReq);
		resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	[Fact]
	public async Task CheckEmail_ExistingEmail_ReturnsTrue()
	{
		var resp = await _client.GetAsync("/api/auth/check-email?email=admin@example.com");
		resp.StatusCode.Should().Be(HttpStatusCode.OK);

		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("exists").GetBoolean().Should().BeTrue();
	}

	[Fact]
	public async Task CheckEmail_NonExistingEmail_ReturnsFalse()
	{
		var resp = await _client.GetAsync("/api/auth/check-email?email=nonexistent@example.com");
		resp.StatusCode.Should().Be(HttpStatusCode.OK);

		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("exists").GetBoolean().Should().BeFalse();
	}

	[Fact]
	public async Task Register_MismatchedPasswords_ReturnsBadRequest()
	{
		var reg = new
		{
			email = "test@example.com",
			name = "Test",
			surname = "User",
			password = "Password123!",
			confirmPassword = "DifferentPassword123!"
		};

		var resp = await _client.PostAsJsonAsync("/api/auth/register", reg);
		resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task Register_ValidData_ReturnsTokens()
	{
		var email = $"newuser_{Guid.NewGuid():N}@example.com";
		var reg = new
		{
			email,
			name = "New",
			surname = "User",
			password = "Password123!",
			confirmPassword = "Password123!"
		};

		var resp = await _client.PostAsJsonAsync("/api/auth/register", reg);
		resp.StatusCode.Should().Be(HttpStatusCode.OK);

		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		json.TryGetProperty("accessToken", out var accessToken).Should().BeTrue();
		json.TryGetProperty("refreshToken", out var refreshToken).Should().BeTrue();
		accessToken.GetString().Should().NotBeNullOrWhiteSpace();
		refreshToken.GetString().Should().NotBeNullOrWhiteSpace();
	}
}
