using MediatR;
using Microsoft.EntityFrameworkCore;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Customers.Queries;
using Minimarket.Application.Features.Customers.DTOs;
using Minimarket.Infrastructure.Data;

namespace Minimarket.Application.Features.Customers.Queries;

public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, Result<PagedResult<CustomerDto>>>
{
    private readonly MinimarketDbContext _context;

    public GetAllCustomersQueryHandler(MinimarketDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<CustomerDto>>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(searchLower) ||
                c.DocumentNumber.Contains(searchLower) ||
                (c.Email != null && c.Email.ToLower().Contains(searchLower)) ||
                (c.Phone != null && c.Phone.Contains(searchLower)));
        }

        if (!string.IsNullOrWhiteSpace(request.DocumentType))
        {
            query = query.Where(c => c.DocumentType == request.DocumentType);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var customers = await query
            .OrderBy(c => c.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                DocumentType = c.DocumentType,
                DocumentNumber = c.DocumentNumber,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var pagedResult = PagedResult<CustomerDto>.Create(customers, totalCount, request.Page, request.PageSize);

        return Result<PagedResult<CustomerDto>>.Success(pagedResult);
    }
}

