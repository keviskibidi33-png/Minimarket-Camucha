using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Sales.DTOs;

namespace Minimarket.Application.Features.Sales.Commands;

public class CreateSaleCommand : IRequest<Result<SaleDto>>
{
    public CreateSaleDto Sale { get; set; } = null!;
    public Guid UserId { get; set; }
}

