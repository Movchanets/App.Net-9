using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using System.Threading.Tasks;

namespace API.IntegrationTests;

public class ProfileEndpointsSplitTests : IClassFixture<TestWebApplicationFactory>
{
	private readonly HttpClient _client;

	public ProfileEndpointsSplitTests(TestWebApplicationFactory factory)
	{
		_client = factory.CreateClient();
	}

	private async Task<(string Email, string Password, string AccessToken)> RegisterUserAndGetAccessToken()
	{
		var email = $"test_{Guid.NewGuid():N}@example.com";
		var password = "User123!";
		var reg = new { email, name = "Test", surname = "User", password, confirmPassword = password };
		var resp = await _client.PostAsJsonAsync("/api/auth/register", reg);
		resp.EnsureSuccessStatusCode();
		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		var token = json.GetProperty("accessToken").GetString()!;
		return (email, password, token);
	}

	[Fact]
	public async Task UpdateMyInfo_UpdatesNameSurnameUsername()
	{
		var (_, _, token) = await RegisterUserAndGetAccessToken();
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

		var update = new { name = "John", surname = "Doe", username = "john.doe" };
		var resp = await _client.PutAsJsonAsync("/api/users/me/info", update);
		resp.StatusCode.Should().Be(HttpStatusCode.OK);
		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
		var payload = json.GetProperty("payload");
		payload.GetProperty("name").GetString().Should().Be("John");
		payload.GetProperty("surname").GetString().Should().Be("Doe");
		payload.GetProperty("username").GetString().Should().Be("john.doe");
	}

	[Fact]
	public async Task UpdateMyPhone_UpdatesPhone()
	{
		var (_, _, token) = await RegisterUserAndGetAccessToken();
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

		var update = new { phoneNumber = "+123456789" };
		var resp = await _client.PutAsJsonAsync("/api/users/me/phone", update);
		resp.StatusCode.Should().Be(HttpStatusCode.OK);
		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
		var payload = json.GetProperty("payload");
		payload.GetProperty("phoneNumber").GetString().Should().Be("+123456789");
	}

	[Fact]
	public async Task UpdateMyEmail_UpdatesEmail()
	{
		var (_, _, token) = await RegisterUserAndGetAccessToken();
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

		var newEmail = $"new_{Guid.NewGuid():N}@example.com";
		var update = new { email = newEmail };
		var resp = await _client.PutAsJsonAsync("/api/users/me/email", update);
		resp.StatusCode.Should().Be(HttpStatusCode.OK);
		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
		var payload = json.GetProperty("payload");
		payload.GetProperty("email").GetString().Should().Be(newEmail);
	}
}
