using Minimarket.Domain.Entities;

namespace Minimarket.Domain.Interfaces;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    // Repositorios genéricos (mantener compatibilidad)
    IRepository<Product> Products { get; }
    IRepository<Category> Categories { get; }
    IRepository<Customer> Customers { get; }
    IRepository<Sale> Sales { get; }
    IRepository<SaleDetail> SaleDetails { get; }
    IRepository<InventoryMovement> InventoryMovements { get; }

    // Repositorios específicos
    IProductRepository ProductRepository { get; }
    ICategoryRepository CategoryRepository { get; }
    ICustomerRepository CustomerRepository { get; }
    ISaleRepository SaleRepository { get; }
    IInventoryMovementRepository InventoryMovementRepository { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

