namespace Minimarket.Domain.Entities;

public class PaymentMethodSettings : BaseEntity
{
    public int PaymentMethodId { get; set; } // Referencia al enum PaymentMethod
    public string Name { get; set; } = string.Empty; // Nombre del método (Efectivo, Tarjeta, etc.)
    public bool IsEnabled { get; set; } = true; // Si está habilitado para uso
    public bool RequiresCardDetails { get; set; } = false; // Si requiere detalles de tarjeta (solo para Tarjeta)
    public string? Description { get; set; }
    public int DisplayOrder { get; set; } = 0; // Orden de visualización
}

