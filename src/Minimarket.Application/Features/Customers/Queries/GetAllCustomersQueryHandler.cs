using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Customers.Queries;
using Minimarket.Application.Features.Customers.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Customers.Queries;

public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, Result<PagedResult<CustomerDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllCustomersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<CustomerDto>>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var allCustomers = await _unitOfWork.Customers.GetAllAsync(cancellationToken);
        
        // Aplicar filtros en memoria
        var filteredCustomers = allCustomers.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            filteredCustomers = filteredCustomers.Where(c =>
                c.Name.ToLower().Contains(searchLower) ||
                c.DocumentNumber.Contains(searchLower) ||
                (c.Email != null && c.Email.ToLower().Contains(searchLower)) ||
                (c.Phone != null && c.Phone.Contains(searchLower)));
        }

        if (!string.IsNullOrWhiteSpace(request.DocumentType))
        {
            filteredCustomers = filteredCustomers.Where(c => c.DocumentType == request.DocumentType);
        }

        if (request.IsActive.HasValue)
        {
            filteredCustomers = filteredCustomers.Where(c => c.IsActive == request.IsActive.Value);
        }

        // Ordenar
        var sortedCustomers = filteredCustomers.OrderBy(c => c.Name).ToList();

        var totalCount = sortedCustomers.Count;

        // Aplicar paginación
        var customers = sortedCustomers
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
            .ToList();

        var pagedResult = PagedResult<CustomerDto>.Create(customers, totalCount, request.Page, request.PageSize);

        return Result<PagedResult<CustomerDto>>.Success(pagedResult);
    }
}

