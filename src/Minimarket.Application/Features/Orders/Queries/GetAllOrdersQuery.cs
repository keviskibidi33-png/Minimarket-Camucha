using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Orders.DTOs;

namespace Minimarket.Application.Features.Orders.Queries;

public class GetAllOrdersQuery : IRequest<Result<PagedResult<WebOrderDto>>>
{
    public string? Status { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

