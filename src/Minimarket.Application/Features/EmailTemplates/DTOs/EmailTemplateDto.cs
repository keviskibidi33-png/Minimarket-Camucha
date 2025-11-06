namespace Minimarket.Application.Features.EmailTemplates.DTOs;

public class EmailTemplateDto
{
    public string TemplateType { get; set; } = string.Empty; // "order_confirmation", "order_status_update", "sale_receipt"
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string PromotionImageUrl { get; set; } = string.Empty;
}

