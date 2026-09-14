using System;
using System.Collections.Generic;

namespace MarketPortfolio.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Free"; // Free or Pro
        
        // Navigation Properties
        public Portfolio? Portfolio { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}