using Microsoft.EntityFrameworkCore;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Infrastructure.Data.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(MinimarketDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.Stock <= p.MinimumStock && p.IsActive)
            .OrderBy(p => p.Stock)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.IsActive && 
                       (p.Name.Contains(searchTerm) || 
                        p.Code.Contains(searchTerm) ||
                        (p.Description != null && p.Description.Contains(searchTerm))))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(p => p.Code == code);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task UpdateStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        var product = await GetByIdAsync(productId, cancellationToken)
            ?? throw new KeyNotFoundException($"Product with ID {productId} not found");

        product.Stock += quantity;
        Update(product);
    }
}

