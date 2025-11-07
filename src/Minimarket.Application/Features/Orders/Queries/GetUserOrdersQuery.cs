using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Orders.DTOs;

namespace Minimarket.Application.Features.Orders.Queries;

public class GetUserOrdersQuery : IRequest<Result<List<WebOrderDto>>>
{
    public string UserEmail { get; set; } = string.Empty;
}

