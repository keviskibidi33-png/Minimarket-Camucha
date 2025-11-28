namespace Minimarket.Domain.Entities;

public enum TipoPlantilla
{
    Home = 0,
    ProductoDetalle = 1,
    Generica = 2
}

public class Page : BaseEntity
{
    public string Titulo { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public TipoPlantilla TipoPlantilla { get; set; } = TipoPlantilla.Generica;
    public string? MetaDescription { get; set; }
    public string? Keywords { get; set; }
    public int Orden { get; set; } = 0;
    public bool Activa { get; set; } = true;
    public bool MostrarEnNavbar { get; set; } = false; // Controla si aparece en el navbar (independiente de Activa)

    // Navigation properties
    public virtual ICollection<PageSection> Sections { get; set; } = new List<PageSection>();
}

