using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Settings.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Settings.Queries;

public class GetAllSettingsQueryHandler : IRequestHandler<GetAllSettingsQuery, Result<IEnumerable<SystemSettingsDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllSettingsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<SystemSettingsDto>>> Handle(GetAllSettingsQuery request, CancellationToken cancellationToken)
    {
        var allSettings = await _unitOfWork.SystemSettings.GetAllAsync(cancellationToken);
        
        var filtered = request.Category != null
            ? allSettings.Where(s => s.Category.Equals(request.Category, StringComparison.OrdinalIgnoreCase))
            : allSettings;

        var result = filtered.Select(s => new SystemSettingsDto
        {
            Id = s.Id,
            Key = s.Key,
            Value = s.Value,
            Description = s.Description,
            Category = s.Category,
            IsActive = s.IsActive
        });

        return Result<IEnumerable<SystemSettingsDto>>.Success(result);
    }
}

