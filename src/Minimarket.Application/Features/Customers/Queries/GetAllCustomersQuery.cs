using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Customers.DTOs;

namespace Minimarket.Application.Features.Customers.Queries;

public class GetAllCustomersQuery : IRequest<Result<PagedResult<CustomerDto>>>
{
    public string? SearchTerm { get; set; }
    public string? DocumentType { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

