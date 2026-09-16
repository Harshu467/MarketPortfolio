using MarketPortfolio.Application.Interfaces;
using MarketPortfolio.Application.Services;
using MarketPortfolio.Domain.Entities;
using Moq;
using Xunit;

namespace MarketPortfolio.UnitTests
{
    public class PortfolioManagerServiceTests // Updated class name
    {
        [Fact]
        public async Task AddStockToPortfolio_WhenFreeTierLimitReached_ThrowsInvalidOperationException()
        {
            // --- 1. ARRANGE ---
            var userId = Guid.NewGuid();

            var mockUserRepository = new Mock<IUserRepository>();
            var mockPortfolioRepository = new Mock<IPortfolioRepository>();

            mockUserRepository
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { Id = userId, Role = "Free" });

            var existingPortfolio = new Portfolio
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Stocks = new List<Stock>
                {
                    new() { TickerSymbol = "AAPL", SharesOwned = 10, PurchasePrice = 150m },
                    new() { TickerSymbol = "MSFT", SharesOwned = 5, PurchasePrice = 300m },
                    new() { TickerSymbol = "GOOG", SharesOwned = 8, PurchasePrice = 140m }
                }
            };

            mockPortfolioRepository
                .Setup(repo => repo.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPortfolio);

            // Using the renamed service class
            var service = new PortfolioManagerService(
                mockPortfolioRepository.Object, 
                mockUserRepository.Object
            );

            // --- 2. ACT & 3. ASSERT ---
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AddStockToPortfolioAsync(userId, "AMZN", 180.00m, 10, default)
            );

            Assert.Equal("Free tier limit reached. Please upgrade to Pro to add more stocks.", exception.Message);

            mockPortfolioRepository.Verify(
                repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), 
                Times.Never
            );
        }
    }
}