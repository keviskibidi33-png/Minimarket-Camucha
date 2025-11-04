using MediatR;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Pages.DTOs;
using Minimarket.Application.Features.Pages.Commands;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Pages.Queries;

public class GetPageBySlugQueryHandler : IRequestHandler<GetPageBySlugQuery, Result<PageDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPageBySlugQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PageDto>> Handle(GetPageBySlugQuery request, CancellationToken cancellationToken)
    {
        var pages = await _unitOfWork.Pages.GetAllAsync(cancellationToken);
        var page = pages.FirstOrDefault(p => p.Slug == request.Slug && p.Activa);

        if (page == null)
        {
            throw new NotFoundException("Page", request.Slug);
        }

        var allSections = await _unitOfWork.PageSections.GetAllAsync(cancellationToken);
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

        return Result<PageDto>.Success(pageDto);
    }
}
