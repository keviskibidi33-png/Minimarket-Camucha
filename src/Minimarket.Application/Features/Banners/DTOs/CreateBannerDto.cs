namespace Minimarket.Application.Features.Banners.DTOs;

public class CreateBannerDto
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
    public string? UrlDestino { get; set; }
    public bool AbrirEnNuevaVentana { get; set; } = false;
    public int Tipo { get; set; } = 0; // BannerTipo.Header por defecto
    public int Posicion { get; set; } = 0; // BannerPosicion.Top por defecto
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public bool Activo { get; set; } = true;
    public int Orden { get; set; } = 0;
    public int? AnchoMaximo { get; set; }
    public int? AltoMaximo { get; set; }
    public string? ClasesCss { get; set; }
    public bool SoloMovil { get; set; } = false;
    public bool SoloDesktop { get; set; } = false;
    public int? MaxVisualizaciones { get; set; }
}
