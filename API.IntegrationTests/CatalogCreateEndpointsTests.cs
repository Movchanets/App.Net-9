using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using Infrastructure;
using Infrastructure.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace API.IntegrationTests;

public class CatalogCreateEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
	private readonly TestWebApplicationFactory _factory;
	private readonly HttpClient _client;

	public CatalogCreateEndpointsTests(TestWebApplicationFactory factory)
	{
		_factory = factory;
		_client = factory.CreateClient();
	}

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
		var email = $"seller_{Guid.NewGuid():N}@example.com";
		var password = "User123!";
		var reg = new { email, name = "Test", surname = "Seller", password, confirmPassword = password };
		var resp = await _client.PostAsJsonAsync("/api/auth/register", reg);
		resp.EnsureSuccessStatusCode();
		return (email, password);
	}

	private async Task EnsureUserInRole(string email, string roleName)
	{
		using var scope = _factory.Services.CreateScope();
		var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
		var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<RoleEntity>>();

		var roleExists = await roleManager.RoleExistsAsync(roleName);
		roleExists.Should().BeTrue($"Seeder should create role '{roleName}'");

		var user = await userManager.FindByEmailAsync(email);
		user.Should().NotBeNull();
		var add = await userManager.AddToRoleAsync(user!, roleName);
		add.Succeeded.Should().BeTrue(string.Join("; ", add.Errors.Select(e => e.Description)));
	}

	private async Task<Guid> GetMyDomainUserId(string accessToken)
	{
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
		var resp = await _client.GetAsync("/api/users/me");
		resp.EnsureSuccessStatusCode();
		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		return json.GetProperty("payload").GetProperty("id").GetGuid();
	}

	private async Task EnsureStoreForUser(Guid domainUserId)
	{
		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		var hasStore = await db.Stores.AnyAsync(s => s.UserId == domainUserId);
		if (hasStore)
		{
			return;
		}

		var store = Store.Create(domainUserId, $"Store {Guid.NewGuid():N}");
		db.Stores.Add(store);
		await db.SaveChangesAsync();
	}

	private async Task<Guid> CreateCategoryAsAdmin(string adminToken)
	{
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
		var resp = await _client.PostAsJsonAsync("/api/categories", new { name = "Cat " + Guid.NewGuid().ToString("N"), description = "d" });
		resp.StatusCode.Should().Be(HttpStatusCode.OK);
		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
		return json.GetProperty("payload").GetGuid();
	}

	private async Task<Guid> CreateTagAsAdmin(string adminToken)
	{
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
		var resp = await _client.PostAsJsonAsync("/api/tags", new { name = "Tag " + Guid.NewGuid().ToString("N"), description = "d" });
		resp.StatusCode.Should().Be(HttpStatusCode.OK);
		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
		return json.GetProperty("payload").GetGuid();
	}

	[Fact]
	public async Task PostCategories_AsAdmin_CreatesCategory()
	{
		var adminToken = await LoginAdminAndGetAccessToken();
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

		var resp = await _client.PostAsJsonAsync("/api/categories", new { name = "Test Category", description = "desc" });
		resp.StatusCode.Should().Be(HttpStatusCode.OK);

		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
		json.GetProperty("payload").GetGuid().Should().NotBeEmpty();
	}

	[Fact]
	public async Task PostTags_AsAdmin_CreatesTag()
	{
		var adminToken = await LoginAdminAndGetAccessToken();
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

		var resp = await _client.PostAsJsonAsync("/api/tags", new { name = "Test Tag", description = "desc" });
		resp.StatusCode.Should().Be(HttpStatusCode.OK);

		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
		json.GetProperty("payload").GetGuid().Should().NotBeEmpty();
	}

	[Fact]
	public async Task PostProducts_AsSeller_CreatesProduct()
	{
		var adminToken = await LoginAdminAndGetAccessToken();
		var (email, password) = await RegisterUser();

		// Make the user a seller, then login to get a token with seller permissions.
		await EnsureUserInRole(email, Roles.Seller);
		var sellerToken = await LoginAndGetAccessToken(email, password);

		var sellerUserId = await GetMyDomainUserId(sellerToken);
		await EnsureStoreForUser(sellerUserId);

		var categoryId = await CreateCategoryAsAdmin(adminToken);
		var tagId = await CreateTagAsAdmin(adminToken);

		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sellerToken);
		var req = new
		{
			name = "Test Product",
			description = "desc",
			categoryIds = new[] { categoryId },
			price = 10.5m,
			stockQuantity = 3,
			attributes = new { color = "red" },
			tagIds = new[] { tagId }
		};

		var resp = await _client.PostAsJsonAsync("/api/products", req);
		resp.StatusCode.Should().Be(HttpStatusCode.OK);

		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
		json.GetProperty("payload").GetGuid().Should().NotBeEmpty();
	}
}
