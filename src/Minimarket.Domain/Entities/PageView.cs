namespace Minimarket.Domain.Entities;

public class PageView : BaseEntity
{
    public string PageSlug { get; set; } = string.Empty;
    public string? UserId { get; set; } // Opcional: si el usuario está autenticado
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
}
