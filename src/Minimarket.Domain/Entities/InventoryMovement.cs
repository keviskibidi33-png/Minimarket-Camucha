using Minimarket.Domain.Enums;

namespace Minimarket.Domain.Entities;

public class InventoryMovement : BaseEntity
{
    public Guid ProductId { get; set; }
    public InventoryMovementType Type { get; set; }
    public int Quantity { get; set; } // Positivo para entrada, negativo para salida
    public string? Reason { get; set; } // Razón del movimiento
    public string? Reference { get; set; } // Referencia (ej: número de venta, compra, etc.)
    public Guid? SaleId { get; set; } // Si el movimiento está relacionado con una venta
    public Guid? UserId { get; set; } // Usuario que realizó el movimiento
    public decimal? UnitPrice { get; set; } // Precio unitario al momento del movimiento
    public string? Notes { get; set; } // Notas adicionales

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual Sale? Sale { get; set; }
}

