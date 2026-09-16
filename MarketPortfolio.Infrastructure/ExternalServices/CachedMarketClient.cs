using MarketPortfolio.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace MarketPortfolio.Infrastructure.ExternalServices
{
    public class CachedMarketClient : IExternalMarketClient
    {
        private readonly IExternalMarketClient _innerClient;
        private readonly IDistributedCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5); // Cache for 5 minutes

        // Constructor Injection: We wrap the original client and inject Redis cache
        public CachedMarketClient(IExternalMarketClient innerClient, IDistributedCache cache)
        {
            _innerClient = innerClient;
            _cache = cache;
        }

        public async Task<decimal> GetCurrentStockPriceAsync(string tickerSymbol, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"stock_price_{tickerSymbol.ToUpperInvariant()}";

            // 1. Check if the price exists in Redis cache
            var cachedPriceString = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cachedPriceString) && decimal.TryParse(cachedPriceString, out var cachedPrice))
            {
                // Cache Hit! Returns instantly without touching Alpha Vantage.
                return cachedPrice;
            }

            // 2. Cache Miss: Fall back to the real external API
            var livePrice = await _innerClient.GetCurrentStockPriceAsync(tickerSymbol, cancellationToken);

            // 3. Save the result into Redis with an expiration window to prevent stale data
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheDuration
            };
            
            await _cache.SetStringAsync(cacheKey, livePrice.ToString(), cacheOptions, cancellationToken);

            return livePrice;
        }
    }
}