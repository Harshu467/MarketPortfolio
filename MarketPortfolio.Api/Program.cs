using MarketPortfolio.Application.Interfaces;
using MarketPortfolio.Application.Services;
using MarketPortfolio.Infrastructure.BackgroundJobs;
using MarketPortfolio.Infrastructure.Data;
using MarketPortfolio.Infrastructure.ExternalServices;
using MarketPortfolio.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// --- SWAGGER SETUP (Added these two lines) ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// ---------------------------------------------

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<PortfolioManagerService>();
// Register the background worker
builder.Services.AddHostedService<StockPriceUpdaterWorker>();
// Register the External Market Client using IHttpClientFactory typed client pattern
builder.Services.AddHttpClient<IExternalMarketClient, AlphaVantageMarketClient>();
// 1. Register StackExchange Redis Distributed Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});
builder.Services.AddAuthorization();
// 2. Register the base HTTP client implementation
builder.Services.AddHttpClient<AlphaVantageMarketClient>();
builder.Services.AddScoped<JwtTokenGenerator>();
// 3. Decorator Pattern Registration: 
// When anyone asks for IExternalMarketClient, provide the CachedMarketClient, 
// passing the AlphaVantageMarketClient and IDistributedCache into its constructor.
builder.Services.AddScoped<IExternalMarketClient>(provider =>
{
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var configuration = provider.GetRequiredService<IConfiguration>();
    
    // Instantiate the inner HTTP client
    var httpClient = httpClientFactory.CreateClient(nameof(AlphaVantageMarketClient));
    var innerClient = new AlphaVantageMarketClient(httpClient, configuration);
    
    // Retrieve Redis cache service
    var cache = provider.GetRequiredService<IDistributedCache>();

    return new CachedMarketClient(innerClient, cache);
});

var app = builder.Build();

// --- SWAGGER MIDDLEWARE (Added these two lines) ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// --------------------------------------------------
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
// Add this at the very bottom of MarketPortfolio.Api/Program.cs
public partial class Program { }