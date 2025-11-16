using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace API.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
	private DbConnection? _connection;

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.UseEnvironment("Testing");
		builder.ConfigureAppConfiguration((ctx, cfg) =>
		{
			var dict = new Dictionary<string, string?>
			{
				["JwtSettings:AccessTokenSecret"] = "test_secret_12345678901234567890",
				["JwtSettings:Issuer"] = "TestIssuer",
				["JwtSettings:Audience"] = "TestAudience",
				["JwtSettings:AccessTokenExpirationMinutes"] = "60"
			};
			cfg.AddInMemoryCollection(dict);
		});

		builder.ConfigureServices(services =>
		{
			// Replace DbContext with SQLite in-memory so migrations can run
			var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
			if (descriptor is not null)
			{
				services.Remove(descriptor);
			}

			_connection = new SqliteConnection("DataSource=:memory:");
			_connection.Open();

			services.AddDbContext<AppDbContext>(options =>
			{
				options.UseSqlite(_connection);
			});

			// Build and create schema from the current model (no migrations)
			using var scope = services.BuildServiceProvider().CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
			db.Database.EnsureCreated();
		});
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		_connection?.Dispose();
	}
}
