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
    IRepository<SystemSettings> SystemSettings { get; }
    IRepository<ShippingRate> ShippingRates { get; }
    IRepository<BrandSettings> BrandSettings { get; }
    IRepository<Module> Modules { get; }
    IRepository<RolePermission> RolePermissions { get; }
    IRepository<Sede> Sedes { get; }
    IRepository<Oferta> Ofertas { get; }
    IRepository<Page> Pages { get; }
    IRepository<PageSection> PageSections { get; }
    IRepository<Translation> Translations { get; }
    IRepository<PageView> PageViews { get; }
    IRepository<ProductView> ProductViews { get; }
    IRepository<WebOrder> WebOrders { get; }
    IRepository<WebOrderItem> WebOrderItems { get; }
    IRepository<UserProfile> UserProfiles { get; }
    IRepository<UserPaymentMethod> UserPaymentMethods { get; }
    IRepository<PaymentMethodSettings> PaymentMethodSettings { get; }
    IRepository<UserAddress> UserAddresses { get; }

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
    
    /// <summary>
    /// Ejecuta una operación dentro de una transacción usando la estrategia de ejecución para soportar reintentos.
    /// Esto es necesario cuando se usa EnableRetryOnFailure en la configuración de SQL Server.
    /// </summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken = default);
}

