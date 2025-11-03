using Minimarket.Domain.Entities;

namespace Minimarket.Domain.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByDocumentNumberAsync(string documentNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<bool> ExistsByDocumentAsync(string documentNumber, string documentType, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

