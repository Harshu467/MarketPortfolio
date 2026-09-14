using MarketPortfolio.Application.Interfaces;
using MarketPortfolio.Domain.Entities;
using MarketPortfolio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarketPortfolio.Infrastructure.Repositories
{
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly ApplicationDbContext _context;

        // Dependency Injection: We inject the EF Core context here.
        public PortfolioRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Portfolio?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Portfolios
                .Include(p => p.Stocks) // Eager loading the related data
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Portfolio?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Portfolios
                .Include(p => p.Stocks)
                .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        }

        public async Task AddAsync(Portfolio portfolio, CancellationToken cancellationToken = default)
        {
            await _context.Portfolios.AddAsync(portfolio, cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}