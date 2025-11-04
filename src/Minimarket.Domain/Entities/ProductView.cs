namespace Minimarket.Domain.Entities;

public class ProductView : BaseEntity
{
    public Guid ProductId { get; set; }
    public string? UserId { get; set; } // Opcional
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
}
