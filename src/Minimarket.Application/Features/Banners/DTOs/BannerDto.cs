namespace Minimarket.Application.Features.Banners.DTOs;

public class BannerDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
    public string? UrlDestino { get; set; }
    public bool AbrirEnNuevaVentana { get; set; }
    public int Tipo { get; set; } // BannerTipo como int
    public int Posicion { get; set; } // BannerPosicion como int
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public bool Activo { get; set; }
    public int Orden { get; set; }
    public int? AnchoMaximo { get; set; }
    public int? AltoMaximo { get; set; }
    public string? ClasesCss { get; set; }
    public bool SoloMovil { get; set; }
    public bool SoloDesktop { get; set; }
    public int? MaxVisualizaciones { get; set; }
    public int VisualizacionesActuales { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
