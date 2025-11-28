namespace Minimarket.Application.Features.Sedes.DTOs;

public class UpdateSedeDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Pais { get; set; } = "Perú";
    public decimal Latitud { get; set; }
    public decimal Longitud { get; set; }
    public string? Telefono { get; set; }
    public Dictionary<string, Dictionary<string, string>> Horarios { get; set; } = new();
    public string? LogoUrl { get; set; }
    public bool Estado { get; set; }
    public string? GoogleMapsUrl { get; set; }
}

