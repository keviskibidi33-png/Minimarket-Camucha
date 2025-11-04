using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Settings.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Settings.Queries;

public class GetSettingByKeyQueryHandler : IRequestHandler<GetSettingByKeyQuery, Result<SystemSettingsDto?>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSettingByKeyQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SystemSettingsDto?>> Handle(GetSettingByKeyQuery request, CancellationToken cancellationToken)
    {
        var setting = await _unitOfWork.SystemSettings.FirstOrDefaultAsync(
            s => s.Key == request.Key && s.IsActive,
            cancellationToken);

        if (setting == null)
        {
            return Result<SystemSettingsDto?>.Success(null);
        }

        var result = new SystemSettingsDto
        {
            Id = setting.Id,
            Key = setting.Key,
            Value = setting.Value,
            Description = setting.Description,
            Category = setting.Category,
            IsActive = setting.IsActive
        };

        return Result<SystemSettingsDto?>.Success(result);
    }
}

