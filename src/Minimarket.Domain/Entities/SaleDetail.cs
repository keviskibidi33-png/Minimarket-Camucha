namespace Minimarket.Domain.Entities;

public class SaleDetail : BaseEntity
{
    public Guid SaleId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }

    // Navigation properties
    public virtual Sale Sale { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}

