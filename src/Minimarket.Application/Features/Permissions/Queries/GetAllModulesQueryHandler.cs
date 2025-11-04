using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Permissions.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Permissions.Queries;

public class GetAllModulesQueryHandler : IRequestHandler<GetAllModulesQuery, Result<IEnumerable<ModuleDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllModulesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<ModuleDto>>> Handle(GetAllModulesQuery request, CancellationToken cancellationToken)
    {
        var modules = await _unitOfWork.Modules.GetAllAsync(cancellationToken);

        var result = modules
            .Where(m => m.IsActive)
            .OrderBy(m => m.Nombre)
            .Select(m => new ModuleDto
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Descripcion = m.Descripcion,
                Slug = m.Slug,
                IsActive = m.IsActive
            })
            .ToList();

        return Result<IEnumerable<ModuleDto>>.Success(result);
    }
}

