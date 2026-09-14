using MarketPortfolio.Domain.Entities;

namespace MarketPortfolio.Application.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task<Subscription?> GetActiveSubscriptionByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}