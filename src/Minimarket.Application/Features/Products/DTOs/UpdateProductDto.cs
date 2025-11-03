namespace Minimarket.Application.Features.Products.DTOs;

public class UpdateProductDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int Stock { get; set; }
    public int MinimumStock { get; set; }
    public Guid CategoryId { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
}

