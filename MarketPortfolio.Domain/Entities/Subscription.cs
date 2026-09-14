using System;

namespace MarketPortfolio.Domain.Entities
{
    public class Subscription
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string RazorpaySubscriptionId { get; set; } = string.Empty; // Razorpay specific
        public string Status { get; set; } = "active";
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        // Foreign Key
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}