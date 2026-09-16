using System.Net.Http.Json;
using MarketPortfolio.Api;
using MarketPortfolio.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace MarketPortfolio.UnitTests
{
    public class PortfolioApiIntegrationTests : IAsyncLifetime
    {
        // Spawns a real, isolated PostgreSQL container via Docker
        private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:16-alpine")
            .Build();

        private WebApplicationFactory<Program> _factory = null!;
        protected HttpClient Client = null!;

        public async Task InitializeAsync()
        {
            await _postgresContainer.StartAsync();

            _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the production PostgreSQL DbContext registration
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    // Point EF Core to our isolated Testcontainer database string
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseNpgsql(_postgresContainer.GetConnectionString());
                    });
                });
            });

            Client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            await _factory.DisposeAsync();
            await _postgresContainer.StopAsync();
        }

        [Fact]
        public async Task AddStock_Endpoint_ReturnsSuccess_WhenWithinLimit()
        {
            // --- ARRANGE ---
            var newStockRequest = new
            {
                Ticker = "AAPL",
                PurchasePrice = 150.00m,
                Shares = 10
            };

            // --- ACT ---
            var response = await Client.PostAsJsonAsync("/api/portfolio/stocks", newStockRequest);

            // --- ASSERT ---
            Assert.True(response.IsSuccessStatusCode);
        }
    }
}