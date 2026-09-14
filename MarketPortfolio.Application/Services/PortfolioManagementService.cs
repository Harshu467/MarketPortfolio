using MarketPortfolio.Application.Interfaces;
using MarketPortfolio.Domain.Entities;

namespace MarketPortfolio.Application.Services
{
    public class PortfolioManagementService
    {
        private readonly IPortfolioRepository _portfolioRepository;
        private readonly IUserRepository _userRepository;

        public PortfolioManagementService(IPortfolioRepository portfolioRepository, IUserRepository userRepository)
        {
            _portfolioRepository = portfolioRepository;
            _userRepository = userRepository;
        }

        public async Task AddStockToPortfolioAsync(Guid userId, string ticker, decimal purchasePrice, int shares, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null) throw new UnauthorizedAccessException("User not found.");

            var portfolio = await _portfolioRepository.GetByUserIdAsync(userId, cancellationToken);
            if (portfolio == null) throw new InvalidOperationException("Portfolio not found.");

            // Core Business Rule: Free users can only track 3 stocks.
            if (user.Role == "Free" && portfolio.Stocks.Count >= 3)
            {
                throw new InvalidOperationException("Free tier limit reached. Please upgrade to Pro to add more stocks.");
            }

            // Aggregate Root usage: We add the stock to the Portfolio entity, not to a "StockRepository"
            var newStock = new Stock 
            { 
                TickerSymbol = ticker, 
                PurchasePrice = purchasePrice, 
                SharesOwned = shares,
                PortfolioId = portfolio.Id
            };

            portfolio.Stocks.Add(newStock);

            // Saving the aggregate root saves the child entities automatically via EF Core tracking
            await _portfolioRepository.SaveChangesAsync(cancellationToken);
        }
    }
}