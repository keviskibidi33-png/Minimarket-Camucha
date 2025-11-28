namespace Minimarket.Domain.Entities;

public enum BannerTipo
{
    Header = 0,      // Banner en el header
    Sidebar = 1,     // Banner en la barra lateral
    Footer = 2,      // Banner en el footer
    Popup = 3,       // Banner popup/modal
    Carousel = 4,    // Banner en carrusel
    Inline = 5       // Banner inline en contenido
}

public enum BannerPosicion
{
    Top = 0,         // Arriba
    Middle = 1,      // Medio
    Bottom = 2,      // Abajo
    Left = 3,        // Izquierda
    Right = 4,       // Derecha
    Center = 5       // Centro
}

public class Banner : BaseEntity
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
    public string? UrlDestino { get; set; } // URL a la que redirige el banner
    public bool AbrirEnNuevaVentana { get; set; } = false;
    public BannerTipo Tipo { get; set; } = BannerTipo.Header;
    public BannerPosicion Posicion { get; set; } = BannerPosicion.Top;
    public DateTime? FechaInicio { get; set; } // Fecha de inicio de visualización
    public DateTime? FechaFin { get; set; } // Fecha de fin de visualización
    public bool Activo { get; set; } = true;
    public int Orden { get; set; } = 0; // Orden de visualización
    public int? AnchoMaximo { get; set; } // Ancho máximo en píxeles (opcional)
    public int? AltoMaximo { get; set; } // Alto máximo en píxeles (opcional)
    public string? ClasesCss { get; set; } // Clases CSS personalizadas
    public bool SoloMovil { get; set; } = false; // Solo mostrar en dispositivos móviles
    public bool SoloDesktop { get; set; } = false; // Solo mostrar en desktop
    public int? MaxVisualizaciones { get; set; } // Límite de visualizaciones (opcional)
    public int VisualizacionesActuales { get; set; } = 0; // Contador de visualizaciones
    public bool IsDeleted { get; set; } = false; // Soft delete: marca como eliminado sin borrar físicamente
    public DateTime? DeletedAt { get; set; } // Fecha de eliminación lógica

    // Método para verificar si el banner debe mostrarse
    public bool DebeMostrarse(DateTime fechaActual)
    {
        if (!Activo) return false;

        // Verificar fechas
        if (FechaInicio.HasValue && fechaActual < FechaInicio.Value)
            return false;

        if (FechaFin.HasValue && fechaActual > FechaFin.Value)
            return false;

        // Verificar límite de visualizaciones
        if (MaxVisualizaciones.HasValue && VisualizacionesActuales >= MaxVisualizaciones.Value)
            return false;

        return true;
    }

    // Incrementar contador de visualizaciones
    public void IncrementarVisualizacion()
    {
        if (!MaxVisualizaciones.HasValue || VisualizacionesActuales < MaxVisualizaciones.Value)
        {
            VisualizacionesActuales++;
        }
    }
}
