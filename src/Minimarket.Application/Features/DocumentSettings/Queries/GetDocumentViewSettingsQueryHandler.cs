using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.DocumentSettings.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.DocumentSettings.Queries;

public class GetDocumentViewSettingsQueryHandler : IRequestHandler<GetDocumentViewSettingsQuery, Result<DocumentViewSettingsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDocumentViewSettingsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DocumentViewSettingsDto>> Handle(GetDocumentViewSettingsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var allSettings = await _unitOfWork.SystemSettings.GetAllAsync(cancellationToken);
            
            // Obtener configuración de modo de visualización
            var viewModeSetting = allSettings.FirstOrDefault(s => s.Key == "document_default_view_mode" && s.IsActive);
            var directPrintSetting = allSettings.FirstOrDefault(s => s.Key == "document_direct_print" && s.IsActive);
            var boletaTemplateSetting = allSettings.FirstOrDefault(s => s.Key == "document_boleta_template_active" && s.IsActive);
            var facturaTemplateSetting = allSettings.FirstOrDefault(s => s.Key == "document_factura_template_active" && s.IsActive);

            var dto = new DocumentViewSettingsDto
            {
                DefaultViewMode = viewModeSetting?.Value?.ToLower() == "direct" ? "direct" : "preview",
                DirectPrint = directPrintSetting?.Value?.ToLower() == "true" || directPrintSetting?.Value == "1",
                BoletaTemplateActive = boletaTemplateSetting == null || boletaTemplateSetting.Value?.ToLower() != "false",
                FacturaTemplateActive = facturaTemplateSetting == null || facturaTemplateSetting.Value?.ToLower() != "false"
            };

            return Result<DocumentViewSettingsDto>.Success(dto);
        }
        catch (Exception)
        {
            // En caso de error, retornar valores por defecto seguros
            return Result<DocumentViewSettingsDto>.Success(new DocumentViewSettingsDto
            {
                DefaultViewMode = "preview",
                DirectPrint = false,
                BoletaTemplateActive = true,
                FacturaTemplateActive = true
            });
        }
    }
}

