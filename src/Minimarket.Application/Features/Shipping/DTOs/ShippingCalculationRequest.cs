namespace Minimarket.Application.Features.Shipping.DTOs;

public class ShippingCalculationRequest
{
    public decimal Subtotal { get; set; }
    public decimal TotalWeight { get; set; } // Peso total en kg
    public decimal Distance { get; set; } // Distancia en km
    public string? ZoneName { get; set; } // Zona de envío (opcional)
    public string DeliveryMethod { get; set; } = "delivery"; // "delivery" o "pickup"
}

