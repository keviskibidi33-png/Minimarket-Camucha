using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Shipping.DTOs;

namespace Minimarket.Application.Features.Shipping.Queries;

public class GetAllShippingRatesQuery : IRequest<Result<IEnumerable<ShippingRateDto>>>
{
    public bool? OnlyActive { get; set; }
}

