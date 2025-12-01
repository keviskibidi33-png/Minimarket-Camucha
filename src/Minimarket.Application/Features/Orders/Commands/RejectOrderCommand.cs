using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Orders.Commands;

public class RejectOrderCommand : IRequest<Result<bool>>
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

