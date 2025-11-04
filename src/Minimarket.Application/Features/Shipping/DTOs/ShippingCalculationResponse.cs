namespace Minimarket.Application.Features.Shipping.DTOs;

public class ShippingCalculationResponse
{
    public decimal ShippingCost { get; set; }
    public string? ZoneName { get; set; }
    public bool IsFreeShipping { get; set; }
    public string? FreeShippingReason { get; set; }
    public string CalculationDetails { get; set; } = string.Empty; // Detalles del cálculo
}

