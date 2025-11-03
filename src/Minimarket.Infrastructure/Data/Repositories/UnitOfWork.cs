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

