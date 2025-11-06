using MediatR;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Shipping.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Shipping.Queries;

public class GetShippingRateByIdQueryHandler : IRequestHandler<GetShippingRateByIdQuery, Result<ShippingRateDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetShippingRateByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ShippingRateDto>> Handle(GetShippingRateByIdQuery request, CancellationToken cancellationToken)
    {
        var shippingRate = await _unitOfWork.ShippingRates.GetByIdAsync(request.Id, cancellationToken);
        
        if (shippingRate == null)
        {
            throw new NotFoundException($"Shipping rate with ID {request.Id} not found");
        }

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

