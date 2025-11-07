namespace Minimarket.Domain.Entities;

public class BrandSettings : BaseEntity
{
    public string LogoUrl { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string? FaviconUrl { get; set; }
    public string PrimaryColor { get; set; } = "#4CAF50"; // Verde por defecto
    public string SecondaryColor { get; set; } = "#0d7ff2"; // Azul por defecto
    public string ButtonColor { get; set; } = "#4CAF50";
    public string TextColor { get; set; } = "#333333";
    public string HoverColor { get; set; } = "#45a049";
    public string? Description { get; set; }
    public string? Slogan { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Ruc { get; set; }
    public Guid UpdatedBy { get; set; }
}

