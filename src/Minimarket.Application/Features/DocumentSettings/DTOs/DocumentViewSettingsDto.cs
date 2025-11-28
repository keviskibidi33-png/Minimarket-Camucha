namespace Minimarket.Application.Features.DocumentSettings.DTOs;

public class DocumentViewSettingsDto
{
    /// <summary>
    /// Modo de visualización predeterminado: "preview" (mostrar vista previa) o "direct" (abrir PDF directamente)
    /// </summary>
    public string DefaultViewMode { get; set; } = "preview";
    
    /// <summary>
    /// Si es true, omite la vista previa y genera/imprime directamente
    /// </summary>
    public bool DirectPrint { get; set; } = false;
    
    /// <summary>
    /// Si es true, la plantilla de Boleta está activa
    /// </summary>
    public bool BoletaTemplateActive { get; set; } = true;
    
    /// <summary>
    /// Si es true, la plantilla de Factura está activa
    /// </summary>
    public bool FacturaTemplateActive { get; set; } = true;
}

