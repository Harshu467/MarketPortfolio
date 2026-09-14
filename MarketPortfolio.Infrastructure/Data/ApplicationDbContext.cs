using MarketPortfolio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketPortfolio.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets represent the tables in PostgreSQL
        public DbSet<User> Users { get; set; }
        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fluent API configurations to strictly define relationships and table constraints

            // 1-to-1 Relationship: User to Portfolio
            modelBuilder.Entity<User>()
                .HasOne(u => u.Portfolio)
                .WithOne(p => p.User)
                .HasForeignKey<Portfolio>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Deleting user deletes portfolio

            // 1-to-Many Relationship: User to Subscriptions
            modelBuilder.Entity<User>()
                .HasMany(u => u.Subscriptions)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1-to-Many Relationship: Portfolio to Stocks
            modelBuilder.Entity<Portfolio>()
                .HasMany(p => p.Stocks)
                .WithOne(s => s.Portfolio)
                .HasForeignKey(s => s.PortfolioId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Ensure TickerSymbols are stored consistently
            modelBuilder.Entity<Stock>()
                .Property(s => s.TickerSymbol)
                .HasMaxLength(10)
                .IsRequired();
                
            // Precision for financial data in PostgreSQL
            modelBuilder.Entity<Stock>()
                .Property(s => s.PurchasePrice)
                .HasColumnType("decimal(18,2)");

            // Seed a test user so we can test the API immediately
            var testUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
           var testPortfolioId = Guid.Parse("00000000-0000-0000-0000-000000000002"); // <-- FIXED

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = testUserId,
                Email = "test@example.com",
                PasswordHash = "hashed_password_here",
                Role = "Free"
            });

            modelBuilder.Entity<Portfolio>().HasData(new Portfolio
            {
                Id = testPortfolioId,
                UserId = testUserId
            });
        }
    }
}