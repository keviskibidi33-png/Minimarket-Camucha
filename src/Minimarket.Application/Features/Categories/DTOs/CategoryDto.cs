namespace Minimarket.Application.Features.Categories.DTOs;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? IconoUrl { get; set; }
    public int Orden { get; set; }
    public bool IsActive { get; set; }
}

