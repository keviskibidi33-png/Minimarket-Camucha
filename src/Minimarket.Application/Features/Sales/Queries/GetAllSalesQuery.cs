using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Sales.DTOs;

namespace Minimarket.Application.Features.Sales.Queries;

public class GetAllSalesQuery : IRequest<Result<PagedResult<SaleDto>>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? UserId { get; set; }
    public string? DocumentNumber { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

