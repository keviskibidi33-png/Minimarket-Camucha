namespace Minimarket.Application.Features.Ofertas.DTOs;

public class UpdateOfertaDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int DescuentoTipo { get; set; }
    public decimal DescuentoValor { get; set; }
    public List<Guid> CategoriasIds { get; set; } = new();
    public List<Guid> ProductosIds { get; set; } = new();
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public bool Activa { get; set; }
    public int Orden { get; set; }
    public string? ImagenUrl { get; set; }
}

