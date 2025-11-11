using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Orders.Commands;

public class UpdatePaymentProofCommand : IRequest<Result<bool>>
{
    public Guid OrderId { get; set; }
    public string PaymentProofUrl { get; set; } = string.Empty;
}

