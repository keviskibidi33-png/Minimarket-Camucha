using Microsoft.EntityFrameworkCore;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Enums;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Infrastructure.Data.Repositories;

public class SaleRepository : Repository<Sale>, ISaleRepository
{
    public SaleRepository(MinimarketDbContext context) : base(context)
    {
    }

    public async Task<Sale?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.SaleDetails)
                .ThenInclude(sd => sd.Product)
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Customer)
            .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Sale?> GetByDocumentNumberAsync(string documentNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.SaleDetails)
                .ThenInclude(sd => sd.Product)
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.DocumentNumber == documentNumber, cancellationToken);
    }

    public async Task<string> GetNextDocumentNumberAsync(DocumentType documentType, CancellationToken cancellationToken = default)
    {
        var prefix = documentType == DocumentType.Factura ? "F" : "B";
        var series = "001";

        var lastDocument = await _dbSet
            .Where(s => s.DocumentType == documentType)
            .OrderByDescending(s => s.DocumentNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastDocument == null)
        {
            return $"{prefix}{series}-00000001";
        }

        // Extraer número del último documento (ej: B001-00000045)
        var parts = lastDocument.DocumentNumber.Split('-');
        if (parts.Length != 2 || !int.TryParse(parts[1], out var lastNumber))
        {
            // Si el formato no es el esperado, buscar el siguiente número disponible
            var existingNumbers = await _dbSet
                .Where(s => s.DocumentType == documentType)
                .Select(s => s.DocumentNumber)
                .ToListAsync(cancellationToken);

            var numbers = existingNumbers
                .Where(n => n.StartsWith($"{prefix}{series}-"))
                .Select(n =>
                {
                    var numPart = n.Split('-').LastOrDefault();
                    return int.TryParse(numPart, out var num) ? num : 0;
                })
                .Where(n => n > 0)
                .ToList();

            lastNumber = numbers.Any() ? numbers.Max() : 0;
        }

        var newNumber = lastNumber + 1;
        return $"{prefix}{series}-{newNumber:D8}";
    }

    public async Task<decimal> GetTotalSalesAmountAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(s => s.Status == SaleStatus.Pagado);

        if (startDate.HasValue)
        {
            query = query.Where(s => s.SaleDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(s => s.SaleDate <= endDate.Value);
        }

        return await query.SumAsync(s => s.Total, cancellationToken);
    }

    public async Task<(IEnumerable<Sale> Sales, int TotalCount)> GetPagedSalesAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? customerId = null,
        Guid? userId = null,
        string? documentNumber = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        // Construir query base con Eager Loading para evitar N+1
        var query = _dbSet
            .Include(s => s.Customer)
            .Include(s => s.SaleDetails)
                .ThenInclude(sd => sd.Product)
            .AsQueryable();

        // Aplicar filtros en la base de datos (no en memoria)
        if (startDate.HasValue)
        {
            query = query.Where(s => s.SaleDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(s => s.SaleDate <= endDate.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(s => s.CustomerId == customerId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(s => s.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(documentNumber))
        {
            query = query.Where(s => s.DocumentNumber.Contains(documentNumber));
        }

        // Obtener total antes de paginar
        var totalCount = await query.CountAsync(cancellationToken);

        // Aplicar ordenamiento y paginación
        var sales = await query
            .OrderByDescending(s => s.SaleDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (sales, totalCount);
    }
}

