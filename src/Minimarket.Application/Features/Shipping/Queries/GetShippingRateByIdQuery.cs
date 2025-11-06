using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Shipping.DTOs;

namespace Minimarket.Application.Features.Shipping.Queries;

public class GetShippingRateByIdQuery : IRequest<Result<ShippingRateDto>>
{
    public Guid Id { get; set; }
}

