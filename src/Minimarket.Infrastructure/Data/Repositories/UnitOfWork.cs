using Microsoft.EntityFrameworkCore.Storage;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;
using Minimarket.Infrastructure.Data.Repositories;

namespace Minimarket.Infrastructure.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly MinimarketDbContext _context;
    private IDbContextTransaction? _transaction;

    // Repositorios genéricos (mantener compatibilidad)
    private IRepository<Product>? _products;
    private IRepository<Category>? _categories;
    private IRepository<Customer>? _customers;
    private IRepository<Sale>? _sales;
    private IRepository<SaleDetail>? _saleDetails;
    private IRepository<InventoryMovement>? _inventoryMovements;
    private IRepository<SystemSettings>? _systemSettings;
    private IRepository<ShippingRate>? _shippingRates;
    private IRepository<Banner>? _banners;
    private IRepository<BrandSettings>? _brandSettings;
    private IRepository<Module>? _modules;
    private IRepository<RolePermission>? _rolePermissions;
    private IRepository<Sede>? _sedes;
    private IRepository<Oferta>? _ofertas;
    private IRepository<Page>? _pages;
    private IRepository<PageSection>? _pageSections;
    private IRepository<Translation>? _translations;
    private IRepository<PageView>? _pageViews;
    private IRepository<ProductView>? _productViews;
    private IRepository<WebOrder>? _webOrders;
    private IRepository<WebOrderItem>? _webOrderItems;

    // Repositorios específicos
    private IProductRepository? _productRepository;
    private ICategoryRepository? _categoryRepository;
    private ICustomerRepository? _customerRepository;
    private ISaleRepository? _saleRepository;
    private IInventoryMovementRepository? _inventoryMovementRepository;

    public UnitOfWork(MinimarketDbContext context)
    {
        _context = context;
    }

    // Repositorios genéricos
    public IRepository<Product> Products =>
        _products ??= new Repository<Product>(_context);

    public IRepository<Category> Categories =>
        _categories ??= new Repository<Category>(_context);

    public IRepository<Customer> Customers =>
        _customers ??= new Repository<Customer>(_context);

    public IRepository<Sale> Sales =>
        _sales ??= new Repository<Sale>(_context);

    public IRepository<SaleDetail> SaleDetails =>
        _saleDetails ??= new Repository<SaleDetail>(_context);

    public IRepository<InventoryMovement> InventoryMovements =>
        _inventoryMovements ??= new Repository<InventoryMovement>(_context);

    public IRepository<SystemSettings> SystemSettings =>
        _systemSettings ??= new Repository<SystemSettings>(_context);

    public IRepository<ShippingRate> ShippingRates =>
        _shippingRates ??= new Repository<ShippingRate>(_context);

    public IRepository<Banner> Banners =>
        _banners ??= new Repository<Banner>(_context);

    public IRepository<BrandSettings> BrandSettings =>
        _brandSettings ??= new Repository<BrandSettings>(_context);

    public IRepository<Module> Modules =>
        _modules ??= new Repository<Module>(_context);

    public IRepository<RolePermission> RolePermissions =>
        _rolePermissions ??= new Repository<RolePermission>(_context);

    public IRepository<Sede> Sedes =>
        _sedes ??= new Repository<Sede>(_context);

    public IRepository<Oferta> Ofertas =>
        _ofertas ??= new Repository<Oferta>(_context);

    public IRepository<Page> Pages =>
        _pages ??= new Repository<Page>(_context);

    public IRepository<PageSection> PageSections =>
        _pageSections ??= new Repository<PageSection>(_context);

    public IRepository<Translation> Translations =>
        _translations ??= new Repository<Translation>(_context);

    public IRepository<PageView> PageViews =>
        _pageViews ??= new Repository<PageView>(_context);

    public IRepository<ProductView> ProductViews =>
        _productViews ??= new Repository<ProductView>(_context);

    public IRepository<WebOrder> WebOrders =>
        _webOrders ??= new Repository<WebOrder>(_context);

    public IRepository<WebOrderItem> WebOrderItems =>
        _webOrderItems ??= new Repository<WebOrderItem>(_context);

    // Repositorios específicos
    public IProductRepository ProductRepository =>
        _productRepository ??= new ProductRepository(_context);

    public ICategoryRepository CategoryRepository =>
        _categoryRepository ??= new CategoryRepository(_context);

    public ICustomerRepository CustomerRepository =>
        _customerRepository ??= new CustomerRepository(_context);

    public ISaleRepository SaleRepository =>
        _saleRepository ??= new SaleRepository(_context);

    public IInventoryMovementRepository InventoryMovementRepository =>
        _inventoryMovementRepository ??= new InventoryMovementRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        // Si ya hay una transacción activa, no crear otra
        if (_transaction != null)
        {
            return;
        }
        
        // NOTA: BeginTransactionAsync no debe usarse dentro de ExecuteAsync
        // porque la estrategia de reintentos no soporta transacciones iniciadas por el usuario.
        // Si necesitas transacciones con reintentos, envuelve toda la operación en ExecuteAsync
        // en lugar de solo el BeginTransactionAsync.
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }
        await _context.DisposeAsync();
    }
}

