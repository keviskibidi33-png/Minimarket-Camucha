namespace Minimarket.Domain.Entities;

public class Banner : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? LinkUrl { get; set; } // URL a donde redirige el banner
    public int DisplayOrder { get; set; } // Orden de visualización
    public string Position { get; set; } = string.Empty; // "home", "category", "product", etc.
    public bool IsActive { get; set; } = true;
    public DateTime? StartDate { get; set; } // Fecha de inicio de visualización
    public DateTime? EndDate { get; set; } // Fecha de fin de visualización
}

