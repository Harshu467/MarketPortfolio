namespace MarketPortfolio.Application.Interfaces
{
    // This abstracts away AlphaVantage or Yahoo Finance. 
    // The business logic doesn't care WHERE the price comes from.
    public interface IExternalMarketClient
    {
        Task<decimal> GetCurrentStockPriceAsync(string tickerSymbol, CancellationToken cancellationToken = default);
    }
}