using MediatR;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Shipping.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Shipping.Commands;

public class UpdateShippingRateCommandHandler : IRequestHandler<UpdateShippingRateCommand, Result<ShippingRateDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateShippingRateCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ShippingRateDto>> Handle(UpdateShippingRateCommand request, CancellationToken cancellationToken)
    {
        var shippingRate = await _unitOfWork.ShippingRates.GetByIdAsync(request.Id, cancellationToken);
        
        if (shippingRate == null)
        {
            throw new NotFoundException($"Shipping rate with ID {request.Id} not found");
        }

        shippingRate.ZoneName = request.ShippingRate.ZoneName;
        shippingRate.BasePrice = request.ShippingRate.BasePrice;
        shippingRate.PricePerKm = request.ShippingRate.PricePerKm;
        shippingRate.PricePerKg = request.ShippingRate.PricePerKg;
        shippingRate.MinDistance = request.ShippingRate.MinDistance;
        shippingRate.MaxDistance = request.ShippingRate.MaxDistance;
        shippingRate.MinWeight = request.ShippingRate.MinWeight;
        shippingRate.MaxWeight = request.ShippingRate.MaxWeight;
        shippingRate.FreeShippingThreshold = request.ShippingRate.FreeShippingThreshold;
        shippingRate.IsActive = request.ShippingRate.IsActive;

        // Entity Framework detecta cambios automáticamente, no necesitamos Update explícito
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

