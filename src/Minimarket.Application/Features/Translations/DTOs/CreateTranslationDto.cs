namespace Minimarket.Application.Features.Translations.DTOs;

public class CreateTranslationDto
{
    public string Key { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = "general";
}

