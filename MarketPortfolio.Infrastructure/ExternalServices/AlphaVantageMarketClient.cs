using System.Text.Json;
using MarketPortfolio.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MarketPortfolio.Infrastructure.ExternalServices
{
    public class AlphaVantageMarketClient : IExternalMarketClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        // HttpClient is injected via IHttpClientFactory to prevent socket exhaustion
        public AlphaVantageMarketClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // We pull the API key from appsettings.json or user secrets
            _apiKey = configuration["AlphaVantage:ApiKey"] ?? "demo"; 
        }

        public async Task<decimal> GetCurrentStockPriceAsync(string tickerSymbol, CancellationToken cancellationToken = default)
        {
            // Alpha Vantage Global Quote endpoint
            var url = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={tickerSymbol}&apikey={_apiKey}";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var jsonDoc = await JsonDocument.ParseAsync(jsonStream, default, cancellationToken);
            
            // Navigate the JSON tree to extract the price field ("05. price")
            if (jsonDoc.RootElement.TryGetProperty("Global Quote", out var globalQuote) &&
                globalQuote.TryGetProperty("05. price", out var priceElement) &&
                decimal.TryParse(priceElement.GetString(), out var price))
            {
                return price;
            }

            // Fallback default value if rate-limited or symbol is invalid on the free tier
            return 0m; 
        }
    }
}