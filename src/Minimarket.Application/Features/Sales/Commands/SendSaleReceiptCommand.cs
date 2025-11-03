using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Sales.Commands;

public class SendSaleReceiptCommand : IRequest<Result<bool>>
{
    public Guid SaleId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
}

