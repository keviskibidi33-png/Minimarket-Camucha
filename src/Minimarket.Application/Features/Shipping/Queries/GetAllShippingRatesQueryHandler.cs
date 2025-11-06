using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Shipping.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Shipping.Queries;

public class GetAllShippingRatesQueryHandler : IRequestHandler<GetAllShippingRatesQuery, Result<IEnumerable<ShippingRateDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllShippingRatesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<ShippingRateDto>>> Handle(GetAllShippingRatesQuery request, CancellationToken cancellationToken)
    {
        var shippingRates = await _unitOfWork.ShippingRates.GetAllAsync(cancellationToken);

        if (request.OnlyActive == true)
        {
            shippingRates = shippingRates.Where(sr => sr.IsActive).ToList();
        }

        var dtos = shippingRates.Select(sr => new ShippingRateDto
        {
            Id = sr.Id,
            ZoneName = sr.ZoneName,
            BasePrice = sr.BasePrice,
            PricePerKm = sr.PricePerKm,
            PricePerKg = sr.PricePerKg,
            MinDistance = sr.MinDistance,
            MaxDistance = sr.MaxDistance,
            MinWeight = sr.MinWeight,
            MaxWeight = sr.MaxWeight,
            FreeShippingThreshold = sr.FreeShippingThreshold,
            IsActive = sr.IsActive,
            CreatedAt = sr.CreatedAt,
            UpdatedAt = sr.UpdatedAt
        });

        return Result<IEnumerable<ShippingRateDto>>.Success(dtos);
    }
}

