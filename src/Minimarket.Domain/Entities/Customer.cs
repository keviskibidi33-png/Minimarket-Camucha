namespace Minimarket.Domain.Entities;

public class Customer : BaseEntity
{
    public string DocumentType { get; set; } = string.Empty; // DNI, RUC
    public string DocumentNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}

