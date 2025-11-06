namespace Minimarket.Application.Features.Orders.DTOs;

public class WebOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string ShippingMethod { get; set; } = string.Empty;
    public string? ShippingAddress { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingRegion { get; set; }
    public string? SelectedSedeId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? WalletMethod { get; set; }
    public bool RequiresPaymentProof { get; set; }
    public string Status { get; set; } = string.Empty; // "pending", "confirmed", "preparing", "shipped", "delivered", "cancelled"
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

