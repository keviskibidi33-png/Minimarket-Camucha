namespace Minimarket.Domain.Entities;

public enum SeccionTipo
{
    Banner = 0,
    TextoImagen = 1,
    GridProductos = 2,
    Categorias = 3,
    Galeria = 4,
    Testimonios = 5,
    CTA = 6,
    Newsletter = 7
}

public class PageSection : BaseEntity
{
    public Guid PageId { get; set; }
    public SeccionTipo SeccionTipo { get; set; }
    public int Orden { get; set; } = 0;
    public string DatosJson { get; set; } = "{}"; // JSON con configuración de la sección

    // Navigation properties
    public virtual Page Page { get; set; } = null!;

    // Helper methods
    public Dictionary<string, object> GetDatos()
    {
        if (string.IsNullOrEmpty(DatosJson)) return new Dictionary<string, object>();
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(DatosJson) 
                ?? new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public void SetDatos(Dictionary<string, object> datos)
    {
        DatosJson = System.Text.Json.JsonSerializer.Serialize(datos);
    }
}

