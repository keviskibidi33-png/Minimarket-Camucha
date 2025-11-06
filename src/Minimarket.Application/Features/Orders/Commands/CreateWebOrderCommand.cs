using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Orders.DTOs;

namespace Minimarket.Application.Features.Orders.Commands;

public class CreateWebOrderCommand : IRequest<Result<WebOrderDto>>
{
    public CreateWebOrderDto Order { get; set; } = new();
}

