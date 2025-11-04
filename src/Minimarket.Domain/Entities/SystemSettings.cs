namespace Minimarket.Domain.Entities;

public class SystemSettings : BaseEntity
{
    public string Key { get; set; } = string.Empty; // Ej: "apply_igv_to_cart", "home_banner_url", "category_image_1"
    public string Value { get; set; } = string.Empty; // Valor en JSON o texto
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // "cart", "shipping", "banners", "categories"
    public bool IsActive { get; set; } = true;
}

