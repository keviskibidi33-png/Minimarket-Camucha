namespace Minimarket.Domain.Enums;

public enum InventoryMovementType
{
    Entrada = 1,      // Compra, ajuste positivo
    Salida = 2,       // Venta, ajuste negativo
    Ajuste = 3,       // Ajuste de inventario
    Devolucion = 4    // Devolución de cliente
}

