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
}

