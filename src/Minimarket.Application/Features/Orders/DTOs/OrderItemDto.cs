namespace Minimarket.Application.Features.Orders.DTOs;

public class OrderItemDto
{
    public string ProductId { get; set; } = string.Empty; // Se recibe como string desde frontend
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}

