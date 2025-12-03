namespace Minimarket.API.Models;

public class CreateOfertaRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int DescuentoTipo { get; set; }
    public decimal DescuentoValor { get; set; }
    public List<string>? CategoriasIds { get; set; }
    public List<string>? ProductosIds { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public bool Activa { get; set; } = true;
    public int Orden { get; set; } = 0;
    public string? ImagenUrl { get; set; }
}

