namespace Minimarket.Domain.Entities;

public class Translation : BaseEntity
{
    public string Key { get; set; } = string.Empty; // Ej: "welcome_message", "product.name"
    public string LanguageCode { get; set; } = "es"; // es, en
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = "general"; // general, product, category, etc.

    // Índice único para evitar duplicados
    public string GetUniqueKey() => $"{Key}_{LanguageCode}_{Category}";
}

