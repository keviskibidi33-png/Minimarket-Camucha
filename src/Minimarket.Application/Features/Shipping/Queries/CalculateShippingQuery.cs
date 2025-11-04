using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Shipping.DTOs;

namespace Minimarket.Application.Features.Shipping.Queries;

public class CalculateShippingQuery : IRequest<Result<ShippingCalculationResponse>>
{
    public ShippingCalculationRequest Request { get; set; } = new();
}

