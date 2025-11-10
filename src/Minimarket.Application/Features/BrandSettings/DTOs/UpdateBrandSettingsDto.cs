namespace Minimarket.Application.Features.BrandSettings.DTOs;

public class UpdateBrandSettingsDto
{
    public string LogoUrl { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string? FaviconUrl { get; set; }
    public string PrimaryColor { get; set; } = "#4CAF50";
    public string SecondaryColor { get; set; } = "#0d7ff2";
    public string ButtonColor { get; set; } = "#4CAF50";
    public string TextColor { get; set; } = "#333333";
    public string HoverColor { get; set; } = "#45a049";
    public string? Description { get; set; }
    public string? Slogan { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Ruc { get; set; }
    public string? YapePhone { get; set; }
    public string? PlinPhone { get; set; }
    public string? YapeQRUrl { get; set; }
    public string? PlinQRUrl { get; set; }
    public bool YapeEnabled { get; set; }
    public bool PlinEnabled { get; set; }
    // Cuenta bancaria
    public string? BankName { get; set; }
    public string? BankAccountType { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankCCI { get; set; }
    public bool BankAccountVisible { get; set; }
    // Opciones de envío
    public string DeliveryType { get; set; } = "Ambos";
    public decimal? DeliveryCost { get; set; }
    public string? DeliveryZones { get; set; }
    // Personalización de página principal
    public string? HomeTitle { get; set; }
    public string? HomeSubtitle { get; set; }
    public string? HomeDescription { get; set; }
    public string? HomeBannerImageUrl { get; set; }
}

