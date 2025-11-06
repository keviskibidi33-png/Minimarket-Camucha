using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Shipping.DTOs;

namespace Minimarket.Application.Features.Shipping.Commands;

public class CreateShippingRateCommand : IRequest<Result<ShippingRateDto>>
{
    public CreateShippingRateDto ShippingRate { get; set; } = new();
}

