using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MarketPortfolio.Infrastructure.BackgroundJobs
{
    public class StockPriceUpdaterWorker : BackgroundService
    {
        private readonly ILogger<StockPriceUpdaterWorker> _logger;
        // PeriodicTimer is modern, high-performance, and prevents timer drift
        private readonly PeriodicTimer _timer = new(TimeSpan.FromHours(1));

        public StockPriceUpdaterWorker(ILogger<StockPriceUpdaterWorker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Stock Price Updater Background Service is starting.");

            try
            {
                while (await _timer.WaitForNextTickAsync(stoppingToken))
                {
                    await UpdateAllPortfolioPricesAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Stock Price Updater Service is stopping gracefully.");
            }
        }

        private async Task UpdateAllPortfolioPricesAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running hourly background stock price sync...");
            // Here you would inject a scoped service via an IServiceScopeFactory 
            // and trigger your pricing/caching pipeline.
            await Task.CompletedTask; 
        }
    }
}