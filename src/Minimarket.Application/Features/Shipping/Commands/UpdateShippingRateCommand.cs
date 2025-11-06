using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Shipping.DTOs;

namespace Minimarket.Application.Features.Shipping.Commands;

public class UpdateShippingRateCommand : IRequest<Result<ShippingRateDto>>
{
    public Guid Id { get; set; }
    public UpdateShippingRateDto ShippingRate { get; set; } = new();
}

