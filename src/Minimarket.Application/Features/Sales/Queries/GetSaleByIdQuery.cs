using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Sales.DTOs;

namespace Minimarket.Application.Features.Sales.Queries;

public class GetSaleByIdQuery : IRequest<Result<SaleDto>>
{
    public Guid Id { get; set; }
}

