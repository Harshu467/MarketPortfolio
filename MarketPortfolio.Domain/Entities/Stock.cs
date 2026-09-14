using System;

namespace MarketPortfolio.Domain.Entities
{
    public class Stock
    {
        public Guid Id { get; set; }
        public string TickerSymbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        
        // The price they bought it at, to calculate gains/losses
        public decimal PurchasePrice { get; set; }
        public int SharesOwned { get; set; }
        
        // Foreign Key
        public Guid PortfolioId { get; set; }
        public Portfolio Portfolio { get; set; } = null!;
    }
}