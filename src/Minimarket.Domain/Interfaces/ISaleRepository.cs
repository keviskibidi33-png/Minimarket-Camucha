using Minimarket.Domain.Entities;
using Minimarket.Domain.Enums;

namespace Minimarket.Domain.Interfaces;

public interface ISaleRepository : IRepository<Sale>
{
    Task<Sale?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<Sale?> GetByDocumentNumberAsync(string documentNumber, CancellationToken cancellationToken = default);
    Task<string> GetNextDocumentNumberAsync(DocumentType documentType, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalSalesAmountAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene ventas con filtros aplicados y paginación, optimizado para evitar N+1 queries
    /// </summary>
    Task<(IEnumerable<Sale> Sales, int TotalCount)> GetPagedSalesAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? customerId = null,
        Guid? userId = null,
        string? documentNumber = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
}

