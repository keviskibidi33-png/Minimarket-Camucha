using Microsoft.EntityFrameworkCore;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Enums;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Infrastructure.Data.Repositories;

public class InventoryMovementRepository : Repository<InventoryMovement>, IInventoryMovementRepository
{
    public InventoryMovementRepository(MinimarketDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<InventoryMovement>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(im => im.Product)
            .Where(im => im.ProductId == productId)
            .OrderByDescending(im => im.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InventoryMovement>> GetByTypeAsync(InventoryMovementType type, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(im => im.Product)
            .Where(im => im.Type == type)
            .OrderByDescending(im => im.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InventoryMovement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(im => im.Product)
            .Where(im => im.CreatedAt >= startDate && im.CreatedAt <= endDate)
            .OrderByDescending(im => im.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InventoryMovement>> GetBySaleIdAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(im => im.Product)
            .Where(im => im.SaleId == saleId)
            .OrderByDescending(im => im.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

