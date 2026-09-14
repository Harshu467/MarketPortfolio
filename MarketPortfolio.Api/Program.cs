using MarketPortfolio.Application.Interfaces;
using MarketPortfolio.Application.Services;
using MarketPortfolio.Infrastructure.Data;
using MarketPortfolio.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddScoped<PortfolioManagementService>();

var app = builder.Build();

// --- SWAGGER MIDDLEWARE (Added these two lines) ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// --------------------------------------------------

app.MapControllers();
app.Run();