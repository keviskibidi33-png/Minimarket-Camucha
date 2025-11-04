using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Payments.Commands;

public class ConfirmPaymentCommand : IRequest<Result<bool>>
{
    public string PaymentIntentId { get; set; } = string.Empty;
    public Guid SaleId { get; set; }
}

