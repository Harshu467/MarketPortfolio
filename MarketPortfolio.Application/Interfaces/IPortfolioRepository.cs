using MarketPortfolio.Domain.Entities;

namespace MarketPortfolio.Application.Interfaces
{
    // Interface Segregation Principle: We keep contracts highly focused.
    public interface IPortfolioRepository
    {
        Task<Portfolio?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Portfolio?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task AddAsync(Portfolio portfolio, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}