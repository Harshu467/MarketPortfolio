using MarketPortfolio.Application.Interfaces;
using MarketPortfolio.Domain.Entities;
using MarketPortfolio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarketPortfolio.Infrastructure.Repositories
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Subscription?> GetActiveSubscriptionByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .Where(s => s.UserId == userId && s.Status == "active" && s.EndDate > DateTime.UtcNow)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
        {
            await _context.Subscriptions.AddAsync(subscription, cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}