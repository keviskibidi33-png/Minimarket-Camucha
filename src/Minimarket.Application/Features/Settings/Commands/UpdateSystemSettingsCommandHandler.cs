using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Settings.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Settings.Commands;

public class UpdateSystemSettingsCommandHandler : IRequestHandler<UpdateSystemSettingsCommand, Result<SystemSettingsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSystemSettingsCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SystemSettingsDto>> Handle(UpdateSystemSettingsCommand request, CancellationToken cancellationToken)
    {
        var setting = await _unitOfWork.SystemSettings.FirstOrDefaultAsync(
            s => s.Key == request.Setting.Key,
            cancellationToken);

        if (setting == null)
        {
            // Crear nueva configuración si no existe
            var newSetting = new Domain.Entities.SystemSettings
            {
                Key = request.Setting.Key,
                Value = request.Setting.Value,
                Description = request.Setting.Description ?? string.Empty,
                Category = GetCategoryFromKey(request.Setting.Key),
                IsActive = request.Setting.IsActive
            };

            await _unitOfWork.SystemSettings.AddAsync(newSetting, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<SystemSettingsDto>.Success(new SystemSettingsDto
            {
                Id = newSetting.Id,
                Key = newSetting.Key,
                Value = newSetting.Value,
                Description = newSetting.Description,
                Category = newSetting.Category,
                IsActive = newSetting.IsActive
            });
        }

        // Actualizar configuración existente
        setting.Value = request.Setting.Value;
        if (!string.IsNullOrEmpty(request.Setting.Description))
        {
            setting.Description = request.Setting.Description;
        }
        setting.IsActive = request.Setting.IsActive;

        await _unitOfWork.SystemSettings.UpdateAsync(setting, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SystemSettingsDto>.Success(new SystemSettingsDto
        {
            Id = setting.Id,
            Key = setting.Key,
            Value = setting.Value,
            Description = setting.Description,
            Category = setting.Category,
            IsActive = setting.IsActive
        });
    }

    private string GetCategoryFromKey(string key)
    {
        if (key.Contains("igv") || key.Contains("cart") || key.Contains("tax"))
            return "cart";
        if (key.Contains("shipping") || key.Contains("delivery"))
            return "shipping";
        if (key.Contains("banner"))
            return "banners";
        if (key.Contains("category") || key.Contains("image"))
            return "categories";
        return "general";
    }
}

