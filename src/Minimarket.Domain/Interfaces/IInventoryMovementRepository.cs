using Minimarket.Domain.Entities;
using Minimarket.Domain.Enums;

namespace Minimarket.Domain.Interfaces;

public interface IInventoryMovementRepository : IRepository<InventoryMovement>
{
    Task<IEnumerable<InventoryMovement>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryMovement>> GetByTypeAsync(InventoryMovementType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryMovement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryMovement>> GetBySaleIdAsync(Guid saleId, CancellationToken cancellationToken = default);
}

