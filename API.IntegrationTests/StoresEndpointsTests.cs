using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Entities;
using FluentAssertions;
using Infrastructure;
using Infrastructure.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace API.IntegrationTests;

public class StoresEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
	private readonly TestWebApplicationFactory _factory;
	private readonly HttpClient _client;

	public StoresEndpointsTests(TestWebApplicationFactory factory)
	{
		_factory = factory;
		_client = factory.CreateClient();
	}

	#region Helpers

	private async Task<string> LoginAndGetAccessToken(string email, string password)
	{
		var resp = await _client.PostAsJsonAsync("/api/auth/login", new { email, password });
		resp.EnsureSuccessStatusCode();
		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		return json.GetProperty("accessToken").GetString()!;
	}

	private async Task<string> LoginAdminAndGetAccessToken()
		=> await LoginAndGetAccessToken("admin@example.com", "Qwerty-1!");

	private async Task<(string Email, string Password)> RegisterUser()
	{
		var email = $"storeuser_{Guid.NewGuid():N}@example.com";
		var password = "User123!";
		var reg = new { email, name = "Store", surname = "Owner", password, confirmPassword = password };
		var resp = await _client.PostAsJsonAsync("/api/auth/register", reg);
		resp.EnsureSuccessStatusCode();
		return (email, password);
	}

	private async Task<Guid> GetDomainUserIdByEmail(string email)
	{
		using var scope = _factory.Services.CreateScope();
		var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

		var appUser = await userManager.FindByEmailAsync(email);
		appUser.Should().NotBeNull($"Expected identity user '{email}' to exist");

		if (appUser!.DomainUserId is Guid domainUserId)
		{
			var exists = await db.DomainUsers.AnyAsync(u => u.Id == domainUserId);
			exists.Should().BeTrue("Domain user should exist for the identity user");
			return domainUserId;
		}

		var domainUser = await db.DomainUsers.FirstOrDefaultAsync(u => u.IdentityUserId == appUser.Id);
		domainUser.Should().NotBeNull("Domain user should be created during registration");
		return domainUser!.Id;
	}

	private async Task<Guid?> GetStoreIdByUserId(Guid domainUserId)
	{
		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		var store = await db.Stores.FirstOrDefaultAsync(s => s.UserId == domainUserId);
		return store?.Id;
	}

	private async Task DeleteStoreIfExists(Guid domainUserId)
	{
		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		var store = await db.Stores.FirstOrDefaultAsync(s => s.UserId == domainUserId);
		if (store != null)
		{
			db.Stores.Remove(store);
			await db.SaveChangesAsync();
		}
	}

	#endregion

	#region Create Store Tests

	[Fact]
	public async Task CreateStore_AsAuthenticatedUser_ReturnsSuccess()
	{
		// Arrange
		var (email, password) = await RegisterUser();
		var token = await LoginAndGetAccessToken(email, password);
		var domainUserId = await GetDomainUserIdByEmail(email);
		await DeleteStoreIfExists(domainUserId); // Clean up any existing store

		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

		var request = new { name = "My Test Store", description = "A test store description" };

		// Act
		var response = await _client.PostAsJsonAsync("/api/stores", request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var json = await response.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
		json.GetProperty("payload").GetString().Should().NotBeNullOrWhiteSpace();

		// Verify store exists in database
		var storeId = await GetStoreIdByUserId(domainUserId);
		storeId.Should().NotBeNull("Store should be created in database");
	}

	[Fact]
	public async Task CreateStore_WithoutAuth_ReturnsUnauthorized()
	{
		// Arrange
		_client.DefaultRequestHeaders.Authorization = null;
		var request = new { name = "Test Store", description = "Test" };

		// Act
		var response = await _client.PostAsJsonAsync("/api/stores", request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	[Fact]
	public async Task CreateStore_WhenStoreAlreadyExists_ReturnsBadRequest()
	{
		// Arrange
		var (email, password) = await RegisterUser();
		var token = await LoginAndGetAccessToken(email, password);
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

		// Create first store
		var request1 = new { name = "First Store", description = "First" };
		var response1 = await _client.PostAsJsonAsync("/api/stores", request1);
		response1.StatusCode.Should().Be(HttpStatusCode.OK);

		// Act - Try to create second store
		var request2 = new { name = "Second Store", description = "Second" };
		var response2 = await _client.PostAsJsonAsync("/api/stores", request2);

		// Assert
		response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		var json = await response2.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeFalse();
	}

	#endregion

	#region Get My Store Tests

	[Fact]
	public async Task GetMyStore_WhenStoreExists_ReturnsStoreDetails()
	{
		// Arrange
		var (email, password) = await RegisterUser();
		var token = await LoginAndGetAccessToken(email, password);
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

		// Create store first
		var createRequest = new { name = "My Store For Get", description = "Description" };
		await _client.PostAsJsonAsync("/api/stores", createRequest);

		// Act
		var response = await _client.GetAsync("/api/stores/my");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var json = await response.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
		
		var payload = json.GetProperty("payload");
		payload.GetProperty("name").GetString().Should().Be("My Store For Get");
		payload.GetProperty("description").GetString().Should().Be("Description");
		payload.GetProperty("isVerified").GetBoolean().Should().BeFalse();
		payload.GetProperty("isSuspended").GetBoolean().Should().BeFalse();
	}

	[Fact]
	public async Task GetMyStore_WhenNoStore_ReturnsNullPayload()
	{
		// Arrange
		var (email, password) = await RegisterUser();
		var token = await LoginAndGetAccessToken(email, password);
		var domainUserId = await GetDomainUserIdByEmail(email);
		await DeleteStoreIfExists(domainUserId);
		
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

		// Act
		var response = await _client.GetAsync("/api/stores/my");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var json = await response.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
		json.GetProperty("payload").ValueKind.Should().Be(JsonValueKind.Null);
	}

	[Fact]
	public async Task GetMyStore_WithoutAuth_ReturnsUnauthorized()
	{
		// Arrange
		_client.DefaultRequestHeaders.Authorization = null;

		// Act
		var response = await _client.GetAsync("/api/stores/my");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	#endregion

	#region Admin: Get All Stores Tests

	[Fact]
	public async Task GetAllStores_AsAdmin_ReturnsAllStores()
	{
		// Arrange
		var adminToken = await LoginAdminAndGetAccessToken();
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

		// Act
		var response = await _client.GetAsync("/api/stores?includeUnverified=true");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var json = await response.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
		json.GetProperty("payload").ValueKind.Should().Be(JsonValueKind.Array);
	}

	[Fact]
	public async Task GetAllStores_AsRegularUser_ReturnsForbidden()
	{
		// Arrange
		var (email, password) = await RegisterUser();
		var token = await LoginAndGetAccessToken(email, password);
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

		// Act
		var response = await _client.GetAsync("/api/stores");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
	}

	[Fact]
	public async Task GetAllStores_WithoutAuth_ReturnsUnauthorized()
	{
		// Arrange
		_client.DefaultRequestHeaders.Authorization = null;

		// Act
		var response = await _client.GetAsync("/api/stores");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	#endregion

	#region Admin: Verify Store Tests

	[Fact]
	public async Task VerifyStore_AsAdmin_ReturnsSuccess()
	{
		// Arrange - Create a store as a user
		var (email, password) = await RegisterUser();
		var userToken = await LoginAndGetAccessToken(email, password);
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

		var createRequest = new { name = "Store To Verify", description = "Test" };
		var createResponse = await _client.PostAsJsonAsync("/api/stores", createRequest);
		createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var createJson = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
		var storeId = createJson.GetProperty("payload").GetString();

		// Login as admin
		var adminToken = await LoginAdminAndGetAccessToken();
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

		// Act
		var response = await _client.PostAsync($"/api/stores/{storeId}/verify", null);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var json = await response.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();

		// Verify in database
		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		var store = await db.Stores.FindAsync(Guid.Parse(storeId!));
		store.Should().NotBeNull();
		store!.IsVerified.Should().BeTrue();
	}

	[Fact]
	public async Task VerifyStore_AsRegularUser_ReturnsForbidden()
	{
		// Arrange
		var (email, password) = await RegisterUser();
		var token = await LoginAndGetAccessToken(email, password);
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

		// Act
		var response = await _client.PostAsync($"/api/stores/{Guid.NewGuid()}/verify", null);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
	}

	#endregion

	#region Admin: Suspend/Unsuspend Store Tests

	[Fact]
	public async Task SuspendStore_AsAdmin_ReturnsSuccess()
	{
		// Arrange - Create and verify a store
		var (email, password) = await RegisterUser();
		var userToken = await LoginAndGetAccessToken(email, password);
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

		var createRequest = new { name = "Store To Suspend", description = "Test" };
		var createResponse = await _client.PostAsJsonAsync("/api/stores", createRequest);
		var createJson = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
		var storeId = createJson.GetProperty("payload").GetString();

		// Login as admin and verify first
		var adminToken = await LoginAdminAndGetAccessToken();
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
		await _client.PostAsync($"/api/stores/{storeId}/verify", null);

		// Act - Suspend
		var response = await _client.PostAsync($"/api/stores/{storeId}/suspend", null);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var json = await response.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();

		// Verify in database
		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		var store = await db.Stores.FindAsync(Guid.Parse(storeId!));
		store!.IsSuspended.Should().BeTrue();
	}

	[Fact]
	public async Task UnsuspendStore_AsAdmin_ReturnsSuccess()
	{
		// Arrange - Create, verify, and suspend a store
		var (email, password) = await RegisterUser();
		var userToken = await LoginAndGetAccessToken(email, password);
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

		var createRequest = new { name = "Store To Unsuspend", description = "Test" };
		var createResponse = await _client.PostAsJsonAsync("/api/stores", createRequest);
		var createJson = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
		var storeId = createJson.GetProperty("payload").GetString();

		// Login as admin
		var adminToken = await LoginAdminAndGetAccessToken();
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
		
		// Verify and suspend
		await _client.PostAsync($"/api/stores/{storeId}/verify", null);
		await _client.PostAsync($"/api/stores/{storeId}/suspend", null);

		// Act - Unsuspend
		var response = await _client.PostAsync($"/api/stores/{storeId}/unsuspend", null);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var json = await response.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();

		// Verify in database
		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		var store = await db.Stores.FindAsync(Guid.Parse(storeId!));
		store!.IsSuspended.Should().BeFalse();
	}

	[Fact]
	public async Task SuspendStore_AsRegularUser_ReturnsForbidden()
	{
		// Arrange
		var (email, password) = await RegisterUser();
		var token = await LoginAndGetAccessToken(email, password);
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

		// Act
		var response = await _client.PostAsync($"/api/stores/{Guid.NewGuid()}/suspend", null);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
	}

	#endregion

	#region Update Store Tests

	[Fact]
	public async Task UpdateStore_AsOwner_ReturnsSuccess()
	{
		// Arrange
		var (email, password) = await RegisterUser();
		var token = await LoginAndGetAccessToken(email, password);
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

		// Create store
		var createRequest = new { name = "Original Name", description = "Original Description" };
		var createResponse = await _client.PostAsJsonAsync("/api/stores", createRequest);
		var createJson = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
		var storeId = createJson.GetProperty("payload").GetString();

		// Act - Update store
		var updateRequest = new { name = "Updated Name", description = "Updated Description" };
		var response = await _client.PutAsJsonAsync($"/api/stores/{storeId}", updateRequest);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var json = await response.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();

		// Verify in database
		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		var store = await db.Stores.FindAsync(Guid.Parse(storeId!));
		store!.Name.Should().Be("Updated Name");
		store.Description.Should().Be("Updated Description");
	}

	[Fact]
	public async Task UpdateStore_WithoutAuth_ReturnsUnauthorized()
	{
		// Arrange
		_client.DefaultRequestHeaders.Authorization = null;
		var updateRequest = new { name = "Test", description = "Test" };

		// Act
		var response = await _client.PutAsJsonAsync($"/api/stores/{Guid.NewGuid()}", updateRequest);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	#endregion

	#region Edge Cases

	[Fact]
	public async Task VerifyStore_NonExistent_ReturnsBadRequest()
	{
		// Arrange
		var adminToken = await LoginAdminAndGetAccessToken();
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

		// Act
		var response = await _client.PostAsync($"/api/stores/{Guid.NewGuid()}/verify", null);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		var json = await response.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeFalse();
	}

	// Note: Validation for empty name is handled by FluentValidation at the MediatR pipeline level
	// and throws ValidationException. This is tested in unit tests for validators.

	#endregion
}
