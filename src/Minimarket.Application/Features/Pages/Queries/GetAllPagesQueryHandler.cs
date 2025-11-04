using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Pages.DTOs;
using Minimarket.Application.Features.Pages.Commands;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Pages.Queries;

public class GetAllPagesQueryHandler : IRequestHandler<GetAllPagesQuery, Result<IEnumerable<PageDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllPagesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<PageDto>>> Handle(GetAllPagesQuery request, CancellationToken cancellationToken)
    {
        var pages = await _unitOfWork.Pages.GetAllAsync(cancellationToken);
        var allSections = await _unitOfWork.PageSections.GetAllAsync(cancellationToken);

        var filtered = request.SoloActivas.HasValue && request.SoloActivas.Value
            ? pages.Where(p => p.Activa)
            : pages;

        var result = filtered
            .OrderBy(p => p.Orden)
            .ThenBy(p => p.Titulo)
            .Select(page =>
            {
                var pageDto = CreatePageCommandHandler.MapToDto(page);
                var sections = allSections
                    .Where(s => s.PageId == page.Id)
                    .OrderBy(s => s.Orden)
                    .Select(s => new PageSectionDto
                    {
                        Id = s.Id,
                        PageId = s.PageId,
                        SeccionTipo = (int)s.SeccionTipo,
                        Orden = s.Orden,
                        Datos = s.GetDatos()
                    }).ToList();
                pageDto.Sections = sections;
                return pageDto;
            })
            .ToList();

        return Result<IEnumerable<PageDto>>.Success(result);
    }
}
