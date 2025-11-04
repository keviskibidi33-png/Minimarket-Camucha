namespace Minimarket.Application.Features.Categories.DTOs;

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? IconoUrl { get; set; }
    public int Orden { get; set; }
}

