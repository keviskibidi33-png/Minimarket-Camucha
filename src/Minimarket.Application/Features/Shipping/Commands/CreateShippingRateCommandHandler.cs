using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Shipping.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Shipping.Commands;

public class CreateShippingRateCommandHandler : IRequestHandler<CreateShippingRateCommand, Result<ShippingRateDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateShippingRateCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ShippingRateDto>> Handle(CreateShippingRateCommand request, CancellationToken cancellationToken)
    {
        var shippingRate = new ShippingRate
        {
            ZoneName = request.ShippingRate.ZoneName,
            BasePrice = request.ShippingRate.BasePrice,
            PricePerKm = request.ShippingRate.PricePerKm,
            PricePerKg = request.ShippingRate.PricePerKg,
            MinDistance = request.ShippingRate.MinDistance,
            MaxDistance = request.ShippingRate.MaxDistance,
            MinWeight = request.ShippingRate.MinWeight,
            MaxWeight = request.ShippingRate.MaxWeight,
            FreeShippingThreshold = request.ShippingRate.FreeShippingThreshold,
            IsActive = request.ShippingRate.IsActive
        };

        await _unitOfWork.ShippingRates.AddAsync(shippingRate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new ShippingRateDto
        {
            Id = shippingRate.Id,
            ZoneName = shippingRate.ZoneName,
            BasePrice = shippingRate.BasePrice,
            PricePerKm = shippingRate.PricePerKm,
            PricePerKg = shippingRate.PricePerKg,
            MinDistance = shippingRate.MinDistance,
            MaxDistance = shippingRate.MaxDistance,
            MinWeight = shippingRate.MinWeight,
            MaxWeight = shippingRate.MaxWeight,
            FreeShippingThreshold = shippingRate.FreeShippingThreshold,
            IsActive = shippingRate.IsActive,
            CreatedAt = shippingRate.CreatedAt,
            UpdatedAt = shippingRate.UpdatedAt
        };

        return Result<ShippingRateDto>.Success(dto);
    }
}

