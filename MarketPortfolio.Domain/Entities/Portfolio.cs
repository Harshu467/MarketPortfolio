using System;
using System.Collections.Generic;

namespace MarketPortfolio.Domain.Entities
{
    public class Portfolio
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        // Foreign Key
        public Guid UserId { get; set; }
        public User User { get; set; } = null!; // Required reference navigation to principal
        
        // Navigation Property
        public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
    }
}