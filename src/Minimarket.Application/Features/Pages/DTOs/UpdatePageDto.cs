namespace Minimarket.Application.Features.Pages.DTOs;

public class UpdatePageDto
{
    public string Titulo { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int TipoPlantilla { get; set; }
    public string? MetaDescription { get; set; }
    public string? Keywords { get; set; }
    public int Orden { get; set; }
    public bool Activa { get; set; }
    public List<UpdatePageSectionDto> Sections { get; set; } = new();
}

public class UpdatePageSectionDto
{
    public Guid? Id { get; set; } // null para nuevas secciones
    public int SeccionTipo { get; set; }
    public int Orden { get; set; }
    public Dictionary<string, object> Datos { get; set; } = new();
}
