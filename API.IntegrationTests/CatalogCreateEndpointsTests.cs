using System;
using System.Linq;
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
using Infrastructure.Initializer;
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

	private async Task<(Guid ProductId, string SkuCode, Guid CategoryId)> GetSeededDemoProductAndCategory()
	{
		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

		var category = await db.Categories.FirstAsync(c => c.Slug == "electronics");
		var product = await db.Products
			.Include(p => p.Skus)
			.Include(p => p.ProductCategories)
			.FirstAsync(p => p.Name == "Phone Case (Demo)");

		product.Skus.Should().NotBeEmpty("Seeder should create at least one SKU for demo products");
		var skuCode = product.Skus.First().SkuCode;
		skuCode.Should().NotBeNullOrWhiteSpace();

		return (product.Id, skuCode, category.Id);
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
		var sellerUserId = await GetDomainUserIdByEmail(email);
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

	[Fact]
	public async Task Seeder_InTesting_SeedsDemoCatalogData()
	{
		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

		var adminUser = await db.DomainUsers.FirstOrDefaultAsync(u => u.Email == "admin@example.com");
		adminUser.Should().NotBeNull("Seeder should create the admin domain user in a fresh Testing database");

		var store = await db.Stores.FirstOrDefaultAsync(s => s.UserId == adminUser!.Id);
		store.Should().NotBeNull("Seeder should create an admin store in Development/Testing");
		store!.IsVerified.Should().BeTrue("Seeder verifies the admin demo store");

		var categorySlugs = await db.Categories
			.Where(c => c.Slug == "electronics" || c.Slug == "accessories" || c.Slug == "clothing")
			.Select(c => c.Slug)
			.ToListAsync();
		categorySlugs.Should().BeEquivalentTo(new[] { "electronics", "accessories", "clothing" });

		var tagSlugs = await db.Tags
			.Where(t => t.Slug == "new" || t.Slug == "popular" || t.Slug == "sale")
			.Select(t => t.Slug)
			.ToListAsync();
		tagSlugs.Should().BeEquivalentTo(new[] { "new", "popular", "sale" });

		var productNames = new[] { "Phone Case (Demo)", "Hoodie (Demo)", "USB-C Cable (Demo)" };
		var demoProducts = await db.Products
			.Where(p => p.StoreId == store.Id && productNames.Contains(p.Name))
			.Include(p => p.Skus)
			.Include(p => p.Gallery)
			.ToListAsync();

		demoProducts.Should().HaveCount(3);
		demoProducts.All(p => !string.IsNullOrWhiteSpace(p.BaseImageUrl)).Should().BeTrue();
		demoProducts.Sum(p => p.Skus.Count).Should().BeGreaterThanOrEqualTo(3);
		demoProducts.Sum(p => p.Gallery.Count).Should().BeGreaterThanOrEqualTo(5);

		var seededMediaCount = await db.MediaImages.CountAsync(m => m.StorageKey.StartsWith("seed/catalog/"));
		seededMediaCount.Should().Be(5);
	}

	[Fact]
	public async Task Seeder_Rerun_IsIdempotent_ForDemoCatalogData()
	{
		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

		var adminUser = await db.DomainUsers.FirstOrDefaultAsync(u => u.Email == "admin@example.com");
		adminUser.Should().NotBeNull();
		var store = await db.Stores.FirstOrDefaultAsync(s => s.UserId == adminUser!.Id);
		store.Should().NotBeNull();

		var initialCategoryCount = await db.Categories.CountAsync(c => c.Slug == "electronics" || c.Slug == "accessories" || c.Slug == "clothing");
		var initialTagCount = await db.Tags.CountAsync(t => t.Slug == "new" || t.Slug == "popular" || t.Slug == "sale");
		var initialSeededMediaCount = await db.MediaImages.CountAsync(m => m.StorageKey.StartsWith("seed/catalog/"));
		var initialDemoProductsCount = await db.Products.CountAsync(p => p.StoreId == store!.Id &&
			(p.Name == "Phone Case (Demo)" || p.Name == "Hoodie (Demo)" || p.Name == "USB-C Cable (Demo)"));

		var appBuilder = new ApplicationBuilderWrapper(_factory.Services);
		await SeederDB.SeedDataAsync(appBuilder);

		var afterCategoryCount = await db.Categories.CountAsync(c => c.Slug == "electronics" || c.Slug == "accessories" || c.Slug == "clothing");
		var afterTagCount = await db.Tags.CountAsync(t => t.Slug == "new" || t.Slug == "popular" || t.Slug == "sale");
		var afterSeededMediaCount = await db.MediaImages.CountAsync(m => m.StorageKey.StartsWith("seed/catalog/"));
		var afterDemoProductsCount = await db.Products.CountAsync(p => p.StoreId == store!.Id &&
			(p.Name == "Phone Case (Demo)" || p.Name == "Hoodie (Demo)" || p.Name == "USB-C Cable (Demo)"));

		afterCategoryCount.Should().Be(initialCategoryCount);
		afterTagCount.Should().Be(initialTagCount);
		afterSeededMediaCount.Should().Be(initialSeededMediaCount);
		afterDemoProductsCount.Should().Be(initialDemoProductsCount);
	}

	[Fact]
	public async Task GetCategories_Anonymous_ReturnsSeededCategories()
	{
		_client.DefaultRequestHeaders.Authorization = null;
		var resp = await _client.GetAsync("/api/categories");
		resp.StatusCode.Should().Be(HttpStatusCode.OK);

		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
		var payload = json.GetProperty("payload");
		payload.ValueKind.Should().Be(JsonValueKind.Array);

		var slugs = payload.EnumerateArray().Select(e => e.GetProperty("slug").GetString()).ToList();
		slugs.Should().Contain("electronics");
	}

	[Fact]
	public async Task GetCategoryBySlug_Anonymous_ReturnsCategory()
	{
		_client.DefaultRequestHeaders.Authorization = null;
		var resp = await _client.GetAsync("/api/categories/slug/electronics");
		resp.StatusCode.Should().Be(HttpStatusCode.OK);

		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
		json.GetProperty("payload").GetProperty("slug").GetString().Should().Be("electronics");
	}

	[Fact]
	public async Task GetTags_Anonymous_ReturnsSeededTags()
	{
		_client.DefaultRequestHeaders.Authorization = null;
		var resp = await _client.GetAsync("/api/tags");
		resp.StatusCode.Should().Be(HttpStatusCode.OK);

		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
		var payload = json.GetProperty("payload");
		payload.ValueKind.Should().Be(JsonValueKind.Array);

		var slugs = payload.EnumerateArray().Select(e => e.GetProperty("slug").GetString()).ToList();
		slugs.Should().Contain("new");
	}

	[Fact]
	public async Task GetTagBySlug_Anonymous_ReturnsTag()
	{
		_client.DefaultRequestHeaders.Authorization = null;
		var resp = await _client.GetAsync("/api/tags/slug/new");
		resp.StatusCode.Should().Be(HttpStatusCode.OK);

		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
		json.GetProperty("payload").GetProperty("slug").GetString().Should().Be("new");
	}

	[Fact]
	public async Task GetProducts_Anonymous_ReturnsSeededProducts()
	{
		_client.DefaultRequestHeaders.Authorization = null;
		var resp = await _client.GetAsync("/api/products");
		resp.StatusCode.Should().Be(HttpStatusCode.OK);

		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
		var payload = json.GetProperty("payload");
		payload.ValueKind.Should().Be(JsonValueKind.Array);

		var names = payload.EnumerateArray().Select(e => e.GetProperty("name").GetString()).ToList();
		names.Should().Contain("Phone Case (Demo)");
	}

	[Fact]
	public async Task GetProductById_Anonymous_ReturnsDetailsWithSkusAndGallery()
	{
		var (productId, _, _) = await GetSeededDemoProductAndCategory();

		_client.DefaultRequestHeaders.Authorization = null;
		var resp = await _client.GetAsync($"/api/products/{productId}");
		resp.StatusCode.Should().Be(HttpStatusCode.OK);

		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
		var payload = json.GetProperty("payload");
		payload.GetProperty("id").GetGuid().Should().Be(productId);

		payload.GetProperty("skus").ValueKind.Should().Be(JsonValueKind.Array);
		payload.GetProperty("skus").GetArrayLength().Should().BeGreaterThan(0);

		payload.GetProperty("gallery").ValueKind.Should().Be(JsonValueKind.Array);
		payload.GetProperty("gallery").GetArrayLength().Should().BeGreaterThan(0);

		var firstGallery = payload.GetProperty("gallery").EnumerateArray().First();
		firstGallery.GetProperty("storageKey").GetString().Should().NotBeNullOrWhiteSpace();
		firstGallery.GetProperty("url").GetString().Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task GetProductBySkuCode_Anonymous_ReturnsSameProductAsById()
	{
		var (productId, skuCode, _) = await GetSeededDemoProductAndCategory();

		_client.DefaultRequestHeaders.Authorization = null;
		var resp = await _client.GetAsync($"/api/products/by-sku/{skuCode}");
		resp.StatusCode.Should().Be(HttpStatusCode.OK);

		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
		json.GetProperty("payload").GetProperty("id").GetGuid().Should().Be(productId);
	}

	[Fact]
	public async Task GetProductsByCategory_Anonymous_ReturnsProducts()
	{
		var (_, _, categoryId) = await GetSeededDemoProductAndCategory();

		_client.DefaultRequestHeaders.Authorization = null;
		var resp = await _client.GetAsync($"/api/products/by-category/{categoryId}");
		resp.StatusCode.Should().Be(HttpStatusCode.OK);

		var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
		json.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
		json.GetProperty("payload").ValueKind.Should().Be(JsonValueKind.Array);
		json.GetProperty("payload").GetArrayLength().Should().BeGreaterThan(0);
	}
}
