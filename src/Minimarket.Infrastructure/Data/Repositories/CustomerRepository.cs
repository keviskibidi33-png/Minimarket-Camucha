using Microsoft.EntityFrameworkCore;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Infrastructure.Data.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(MinimarketDbContext context) : base(context)
    {
    }

    public async Task<Customer?> GetByDocumentNumberAsync(string documentNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.DocumentNumber == documentNumber, cancellationToken);
    }

    public async Task<IEnumerable<Customer>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsActive && 
                       (c.Name.Contains(searchTerm) || 
                        c.DocumentNumber.Contains(searchTerm) ||
                        (c.Email != null && c.Email.Contains(searchTerm)) ||
                        (c.Phone != null && c.Phone.Contains(searchTerm))))
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByDocumentAsync(string documentNumber, string documentType, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(c => c.DocumentNumber == documentNumber && c.DocumentType == documentType);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}

