using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.CashClosure.Commands;

public class MarkSalesAsClosedCommand : IRequest<Result<bool>>
{
    public List<Guid> SaleIds { get; set; } = new();
}

