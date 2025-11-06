namespace Minimarket.Application.Features.Shipping.DTOs;

public class CreateShippingRateDto
{
    public string ZoneName { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public decimal PricePerKm { get; set; }
    public decimal PricePerKg { get; set; }
    public decimal MinDistance { get; set; }
    public decimal MaxDistance { get; set; }
    public decimal MinWeight { get; set; }
    public decimal MaxWeight { get; set; }
    public decimal FreeShippingThreshold { get; set; }
    public bool IsActive { get; set; } = true;
}

