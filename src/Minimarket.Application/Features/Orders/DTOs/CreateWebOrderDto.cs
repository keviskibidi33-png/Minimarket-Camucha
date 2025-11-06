namespace Minimarket.Application.Features.Orders.DTOs;

public class CreateWebOrderDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string ShippingMethod { get; set; } = string.Empty; // "delivery" o "pickup"
    public string? ShippingAddress { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingRegion { get; set; }
    public string? SelectedSedeId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // "card", "bank", "wallet", "cash"
    public string? WalletMethod { get; set; } // "yape", "plin", "tunki"
    public bool RequiresPaymentProof { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

