namespace Minimarket.Application.Features.Pages.DTOs;

public class CreatePageDto
{
    public string Titulo { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int TipoPlantilla { get; set; } = 2; // Generica por defecto
    public string? MetaDescription { get; set; }
    public string? Keywords { get; set; }
    public int Orden { get; set; } = 0;
    public bool Activa { get; set; } = true;
    public List<CreatePageSectionDto> Sections { get; set; } = new();
}

public class CreatePageSectionDto
{
    public int SeccionTipo { get; set; }
    public int Orden { get; set; }
    public Dictionary<string, object> Datos { get; set; } = new();
}

