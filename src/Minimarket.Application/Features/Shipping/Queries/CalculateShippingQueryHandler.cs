using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Shipping.DTOs;
using Minimarket.Domain.Entities;
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

        // Obtener todas las tarifas activas
        var allRates = await _unitOfWork.ShippingRates.GetAllAsync(cancellationToken);
        var activeRates = allRates.Where(r => r.IsActive).ToList();

        if (!activeRates.Any())
        {
            // Si no hay tarifas configuradas, usar una tarifa por defecto
            return Result<ShippingCalculationResponse>.Success(new ShippingCalculationResponse
            {
                ShippingCost = CalculateDefaultShipping(req.Subtotal, req.Distance, req.TotalWeight),
                IsFreeShipping = false,
                CalculationDetails = "Tarifa por defecto aplicada (no hay tarifas configuradas)"
            });
        }

        // Buscar tarifa por zona si se especificó
        ShippingRate? selectedRate = null;
        if (!string.IsNullOrEmpty(req.ZoneName))
        {
            selectedRate = activeRates.FirstOrDefault(r => 
                r.ZoneName.Equals(req.ZoneName, StringComparison.OrdinalIgnoreCase));
        }

        // Si no se encontró por zona, buscar por distancia y peso
        if (selectedRate == null)
        {
            selectedRate = activeRates.FirstOrDefault(r =>
                req.Distance >= r.MinDistance &&
                (r.MaxDistance == 0 || req.Distance <= r.MaxDistance) &&
                req.TotalWeight >= r.MinWeight &&
                (r.MaxWeight == 0 || req.TotalWeight <= r.MaxWeight));
        }

        // Si aún no hay tarifa, usar la primera activa o calcular por defecto
        if (selectedRate == null)
        {
            selectedRate = activeRates.FirstOrDefault();
        }

        if (selectedRate == null)
        {
            return Result<ShippingCalculationResponse>.Success(new ShippingCalculationResponse
            {
                ShippingCost = CalculateDefaultShipping(req.Subtotal, req.Distance, req.TotalWeight),
                IsFreeShipping = false,
                CalculationDetails = "Tarifa por defecto aplicada"
            });
        }

        // Verificar si aplica envío gratis por monto mínimo
        bool isFreeShipping = selectedRate.FreeShippingThreshold > 0 && 
                              req.Subtotal >= selectedRate.FreeShippingThreshold;

        if (isFreeShipping)
        {
            return Result<ShippingCalculationResponse>.Success(new ShippingCalculationResponse
            {
                ShippingCost = 0,
                ZoneName = selectedRate.ZoneName,
                IsFreeShipping = true,
                FreeShippingReason = $"Compra mínima de S/ {selectedRate.FreeShippingThreshold:F2}",
                CalculationDetails = $"Envío gratis - Compra mínima alcanzada (S/ {selectedRate.FreeShippingThreshold:F2})"
            });
        }

        // Calcular costo de envío según fórmula
        // Fórmula: BasePrice + (Distance * PricePerKm) + (Weight * PricePerKg)
        decimal shippingCost = selectedRate.BasePrice +
                              (req.Distance * selectedRate.PricePerKm) +
                              (req.TotalWeight * selectedRate.PricePerKg);

        // Asegurar que el costo no sea negativo
        shippingCost = Math.Max(0, shippingCost);

        return Result<ShippingCalculationResponse>.Success(new ShippingCalculationResponse
        {
            ShippingCost = shippingCost,
            ZoneName = selectedRate.ZoneName,
            IsFreeShipping = false,
            CalculationDetails = $"Base: S/ {selectedRate.BasePrice:F2} + " +
                               $"Distancia ({req.Distance:F2} km × S/ {selectedRate.PricePerKm:F2}/km) + " +
                               $"Peso ({req.TotalWeight:F2} kg × S/ {selectedRate.PricePerKg:F2}/kg) = " +
                               $"S/ {shippingCost:F2}"
        });
    }

    private decimal CalculateDefaultShipping(decimal subtotal, decimal distance, decimal weight)
    {
        // Tarifa por defecto basada en datos reales de Lima, Perú
        // Base: S/ 3.50 (costo base de envío)
        // Por km: S/ 0.50 (costo adicional por kilómetro)
        // Por kg: S/ 1.00 (costo adicional por kilogramo)
        // Envío gratis si compra >= S/ 100

        const decimal defaultBasePrice = 3.50m;
        const decimal defaultPricePerKm = 0.50m;
        const decimal defaultPricePerKg = 1.00m;
        const decimal freeShippingThreshold = 100.00m;

        if (subtotal >= freeShippingThreshold)
        {
            return 0;
        }

        decimal cost = defaultBasePrice + (distance * defaultPricePerKm) + (weight * defaultPricePerKg);
        return Math.Max(0, cost);
    }
}

