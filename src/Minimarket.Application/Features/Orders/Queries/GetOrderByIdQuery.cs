using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Orders.DTOs;

namespace Minimarket.Application.Features.Orders.Queries;

public class GetOrderByIdQuery : IRequest<Result<WebOrderDto>>
{
    public Guid OrderId { get; set; }
}

