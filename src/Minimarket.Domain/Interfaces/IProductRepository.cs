using Minimarket.Domain.Entities;

namespace Minimarket.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task UpdateStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
}

