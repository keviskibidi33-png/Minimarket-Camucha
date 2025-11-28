using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Shipping.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Shipping.Queries;

public class CalculateShippingQueryHandler : IRequestHandler<CalculateShippingQuery, Result<ShippingCalculationResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CalculateShippingQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ShippingCalculationResponse>> Handle(CalculateShippingQuery request, CancellationToken cancellationToken)
    {
        var req = request.Request;

        // Si es retiro en tienda, el shipping es gratis
        if (req.DeliveryMethod == "pickup")
        {
            return Result<ShippingCalculationResponse>.Success(new ShippingCalculationResponse
            {
                ShippingCost = 0,
                IsFreeShipping = true,
                FreeShippingReason = "Retiro en tienda",
                CalculationDetails = "Retiro en tienda - Sin costo de envío"
            });
        }

        // Obtener configuración de precio fijo de envío
        var fixedPriceSetting = await _unitOfWork.SystemSettings.FirstOrDefaultAsync(
            s => s.Key == "fixed_shipping_price" && s.IsActive,
            cancellationToken);
        
        // Obtener configuración de umbral de envío gratis
        var freeShippingSetting = await _unitOfWork.SystemSettings.FirstOrDefaultAsync(
            s => s.Key == "free_shipping_threshold" && s.IsActive,
            cancellationToken);

        // Valores por defecto si no están configurados
        decimal fixedShippingPrice = 8.00m;
        decimal freeShippingThreshold = 20.00m;

        if (fixedPriceSetting != null && !string.IsNullOrEmpty(fixedPriceSetting.Value))
        {
            if (decimal.TryParse(fixedPriceSetting.Value, out var parsedPrice))
            {
                fixedShippingPrice = parsedPrice;
            }
        }

        if (freeShippingSetting != null && !string.IsNullOrEmpty(freeShippingSetting.Value))
        {
            if (decimal.TryParse(freeShippingSetting.Value, out var parsedThreshold))
            {
                freeShippingThreshold = parsedThreshold;
            }
        }

        // Verificar si aplica envío gratis
        if (req.Subtotal >= freeShippingThreshold)
        {
            return Result<ShippingCalculationResponse>.Success(new ShippingCalculationResponse
            {
                ShippingCost = 0,
                ZoneName = "Lima",
                IsFreeShipping = true,
                FreeShippingReason = $"Compra mínima de S/ {freeShippingThreshold:F2}",
                CalculationDetails = $"Envío gratis - Compra mínima alcanzada (S/ {freeShippingThreshold:F2})"
            });
        }

        // Aplicar precio fijo
        return Result<ShippingCalculationResponse>.Success(new ShippingCalculationResponse
        {
            ShippingCost = fixedShippingPrice,
            ZoneName = "Lima",
            IsFreeShipping = false,
            CalculationDetails = $"Envío fijo para Lima: S/ {fixedShippingPrice:F2}"
        });
    }
}

