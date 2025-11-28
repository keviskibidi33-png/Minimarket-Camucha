namespace Minimarket.Application.Features.Ofertas.DTOs;

public class CreateOfertaDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int DescuentoTipo { get; set; } // 0 = Porcentaje, 1 = MontoFijo
    public decimal DescuentoValor { get; set; }
    public List<Guid> CategoriasIds { get; set; } = new();
    public List<Guid> ProductosIds { get; set; } = new();
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public bool Activa { get; set; } = true;
    public int Orden { get; set; } = 0;
    public string? ImagenUrl { get; set; }
}

