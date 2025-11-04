namespace Minimarket.Application.Features.BrandSettings.DTOs;

public class BrandSettingsDto
{
    public Guid Id { get; set; }
    public string LogoUrl { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string? FaviconUrl { get; set; }
    public string PrimaryColor { get; set; } = string.Empty;
    public string SecondaryColor { get; set; } = string.Empty;
    public string ButtonColor { get; set; } = string.Empty;
    public string TextColor { get; set; } = string.Empty;
    public string HoverColor { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Slogan { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

