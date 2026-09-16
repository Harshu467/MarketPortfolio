using MarketPortfolio.Application.Interfaces;
using MarketPortfolio.Domain.Entities;

namespace MarketPortfolio.Application.Services
{
    public class PortfolioManagerService
    {
        private readonly IPortfolioRepository _portfolioRepository;
        private readonly IUserRepository _userRepository;
        private readonly IExternalMarketClient? _marketClient;

        public PortfolioManagerService(
            IPortfolioRepository portfolioRepository,
            IUserRepository userRepository,
            IExternalMarketClient marketClient)
        {
            _portfolioRepository = portfolioRepository;
            _userRepository = userRepository;
            _marketClient = marketClient;
        }

        public PortfolioManagerService(
            IPortfolioRepository portfolioRepository,
            IUserRepository userRepository)
            : this(portfolioRepository, userRepository, null!)
        {
        }

        public async Task AddStockToPortfolioAsync(
            Guid userId,
            string ticker,
            decimal purchasePrice,
            int shares,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(ticker))
                throw new InvalidOperationException("Ticker symbol is required.");

            if (purchasePrice <= 0)
                throw new InvalidOperationException("Purchase price must be greater than zero.");

            if (shares <= 0)
                throw new InvalidOperationException("Shares must be greater than zero.");

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                throw new UnauthorizedAccessException("The user does not exist.");

            var portfolio = await _portfolioRepository.GetByUserIdAsync(userId, cancellationToken);
            if (portfolio == null)
                throw new UnauthorizedAccessException("The user does not have a portfolio.");

            if (string.Equals(user.Role, "Free", StringComparison.OrdinalIgnoreCase) &&
                portfolio.Stocks.Count >= 3)
            {
                throw new InvalidOperationException("Free tier limit reached. Please upgrade to Pro to add more stocks.");
            }

            portfolio.Stocks.Add(new Stock
            {
                TickerSymbol = ticker.Trim().ToUpperInvariant(),
                PurchasePrice = purchasePrice,
                SharesOwned = shares,
                PortfolioId = portfolio.Id
            });

            await _portfolioRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task<decimal> CalculateTotalPortfolioValueAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var portfolio = await _portfolioRepository.GetByUserIdAsync(userId, cancellationToken);
            
            if (portfolio == null || !portfolio.Stocks.Any())
                return 0m;

            decimal totalValue = 0m;

            foreach (var stock in portfolio.Stocks)
            {
                // Calls the Alpha Vantage API over HTTP asynchronously for each stock
                if (_marketClient == null)
                    throw new InvalidOperationException("A market client is required to calculate portfolio value.");

                var livePrice = await _marketClient.GetCurrentStockPriceAsync(stock.TickerSymbol, cancellationToken);
                totalValue += (livePrice * stock.SharesOwned);
            }

            return totalValue;
        }
    }
}