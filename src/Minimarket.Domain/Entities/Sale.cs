using Minimarket.Domain.Enums;

namespace Minimarket.Domain.Entities;

public class Sale : BaseEntity
{
    public string DocumentNumber { get; set; } = string.Empty; // F001-00123, B001-00456
    public DocumentType DocumentType { get; set; }
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    public Guid? CustomerId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; } // IGV 18%
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Change { get; set; }
    public SaleStatus Status { get; set; } = SaleStatus.Pendiente;
    public Guid UserId { get; set; } // Cajero
    public string? CancellationReason { get; set; }

    // Navigation properties
    public virtual Customer? Customer { get; set; }
    public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
}

