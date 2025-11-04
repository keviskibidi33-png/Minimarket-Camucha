namespace Minimarket.Domain.Entities;

public class ShippingRate : BaseEntity
{
    public string ZoneName { get; set; } = string.Empty; // Ej: "Lima Centro", "Lima Norte", "Callao"
    public decimal BasePrice { get; set; } // Precio base por km o zona
    public decimal PricePerKm { get; set; } // Precio adicional por kilómetro
    public decimal PricePerKg { get; set; } // Precio adicional por kilogramo
    public decimal MinDistance { get; set; } // Distancia mínima en km
    public decimal MaxDistance { get; set; } // Distancia máxima en km (0 = sin límite)
    public decimal MinWeight { get; set; } // Peso mínimo en kg
    public decimal MaxWeight { get; set; } // Peso máximo en kg (0 = sin límite)
    public decimal FreeShippingThreshold { get; set; } // Monto mínimo para envío gratis (0 = no aplica)
    public bool IsActive { get; set; } = true;
}

