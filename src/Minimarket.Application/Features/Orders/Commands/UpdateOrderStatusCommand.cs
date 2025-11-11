using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Orders.Commands;

public class UpdateOrderStatusCommand : IRequest<Result<bool>>
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TrackingUrl { get; set; }
    public DateTime? EstimatedDelivery { get; set; }
}

