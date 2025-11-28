using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.DocumentSettings.Commands;
using Minimarket.Application.Features.DocumentSettings.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.DocumentSettings.Commands;

public class UpdateDocumentViewSettingsCommandHandler : IRequestHandler<UpdateDocumentViewSettingsCommand, Result<DocumentViewSettingsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateDocumentViewSettingsCommandHandler> _logger;

    public UpdateDocumentViewSettingsCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateDocumentViewSettingsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<DocumentViewSettingsDto>> Handle(UpdateDocumentViewSettingsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Actualizar o crear configuración de modo de visualización
            await UpdateOrCreateSetting(
                "document_default_view_mode",
                request.Settings.DefaultViewMode,
                "Modo de visualización predeterminado para documentos (preview/direct)",
                "DocumentSettings",
                cancellationToken);

            // Actualizar o crear configuración de impresión directa
            await UpdateOrCreateSetting(
                "document_direct_print",
                request.Settings.DirectPrint.ToString().ToLower(),
                "Si es true, omite la vista previa y genera/imprime directamente",
                "DocumentSettings",
                cancellationToken);

            // Actualizar o crear configuración de plantilla Boleta
            await UpdateOrCreateSetting(
                "document_boleta_template_active",
                request.Settings.BoletaTemplateActive.ToString().ToLower(),
                "Si la plantilla de Boleta está activa",
                "DocumentSettings",
                cancellationToken);

            // Actualizar o crear configuración de plantilla Factura
            await UpdateOrCreateSetting(
                "document_factura_template_active",
                request.Settings.FacturaTemplateActive.ToString().ToLower(),
                "Si la plantilla de Factura está activa",
                "DocumentSettings",
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Document view settings updated successfully");

            return Result<DocumentViewSettingsDto>.Success(request.Settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document view settings");
            return Result<DocumentViewSettingsDto>.Failure($"Error al actualizar la configuración: {ex.Message}");
        }
    }

    private async Task UpdateOrCreateSetting(
        string key,
        string value,
        string description,
        string category,
        CancellationToken cancellationToken)
    {
        var setting = await _unitOfWork.SystemSettings.FirstOrDefaultAsync(
            s => s.Key == key,
            cancellationToken);

        if (setting == null)
        {
            setting = new Domain.Entities.SystemSettings
            {
                Key = key,
                Value = value,
                Description = description,
                Category = category,
                IsActive = true
            };
            await _unitOfWork.SystemSettings.AddAsync(setting, cancellationToken);
        }
        else
        {
            setting.Value = value;
            if (!string.IsNullOrEmpty(description))
            {
                setting.Description = description;
            }
            setting.IsActive = true;
            await _unitOfWork.SystemSettings.UpdateAsync(setting, cancellationToken);
        }
    }
}

