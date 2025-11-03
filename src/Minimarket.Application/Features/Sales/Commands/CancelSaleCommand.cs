using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Sales.Commands;

public class CancelSaleCommand : IRequest<Result<bool>>
{
    public Guid SaleId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}

