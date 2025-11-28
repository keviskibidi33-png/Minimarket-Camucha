namespace Minimarket.Application.Features.Pages.DTOs;

public class PageDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int TipoPlantilla { get; set; } // 0 = Home, 1 = ProductoDetalle, 2 = Generica
    public string? MetaDescription { get; set; }
    public string? Keywords { get; set; }
    public int Orden { get; set; }
    public bool Activa { get; set; }
    public bool MostrarEnNavbar { get; set; }
    public List<PageSectionDto> Sections { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PageSectionDto
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }
    public int SeccionTipo { get; set; } // 0-7 (Banner, TextoImagen, etc.)
    public int Orden { get; set; }
    public Dictionary<string, object> Datos { get; set; } = new();
}
