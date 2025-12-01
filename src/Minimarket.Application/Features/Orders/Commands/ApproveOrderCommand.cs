using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Orders.Commands;

public class ApproveOrderCommand : IRequest<Result<bool>>
{
    public Guid OrderId { get; set; }
    public bool SendPaymentVerifiedEmail { get; set; } = false;
}

