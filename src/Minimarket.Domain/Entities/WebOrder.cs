namespace Minimarket.Domain.Entities;

public class WebOrder : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string ShippingMethod { get; set; } = string.Empty; // "delivery" o "pickup"
    public string? ShippingAddress { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingRegion { get; set; }
    public Guid? SelectedSedeId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // "card", "bank", "wallet", "cash"
    public string? WalletMethod { get; set; } // "yape", "plin", "tunki"
    public bool RequiresPaymentProof { get; set; }
    public string Status { get; set; } = "pending"; // "pending", "confirmed", "preparing", "shipped", "delivered", "ready_for_pickup", "cancelled"
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }
    public string? TrackingUrl { get; set; }
    public DateTime? EstimatedDelivery { get; set; }

    // Navigation properties
    public virtual Sede? SelectedSede { get; set; }
    public virtual ICollection<WebOrderItem> OrderItems { get; set; } = new List<WebOrderItem>();
}

public class WebOrderItem : BaseEntity
{
    public Guid WebOrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }

    // Navigation properties
    public virtual WebOrder? WebOrder { get; set; }
    public virtual Product? Product { get; set; }
}

