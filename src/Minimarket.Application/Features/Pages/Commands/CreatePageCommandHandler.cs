using MediatR;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Pages.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Pages.Commands;

public class CreatePageCommandHandler : IRequestHandler<CreatePageCommand, Result<PageDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreatePageCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PageDto>> Handle(CreatePageCommand request, CancellationToken cancellationToken)
    {
        // Verificar que el slug no exista
        var existingPages = await _unitOfWork.Pages.GetAllAsync(cancellationToken);
        if (existingPages.Any(p => p.Slug == request.Page.Slug))
        {
            throw new BusinessRuleViolationException($"Ya existe una página con el slug '{request.Page.Slug}'");
        }

        var page = new Page
        {
            Titulo = request.Page.Titulo,
            Slug = request.Page.Slug,
            TipoPlantilla = (TipoPlantilla)request.Page.TipoPlantilla,
            MetaDescription = request.Page.MetaDescription,
            Keywords = request.Page.Keywords,
            Orden = request.Page.Orden,
            Activa = request.Page.Activa
        };

        await _unitOfWork.Pages.AddAsync(page, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Agregar secciones
        foreach (var sectionDto in request.Page.Sections.OrderBy(s => s.Orden))
        {
            var section = new PageSection
            {
                PageId = page.Id,
                SeccionTipo = (SeccionTipo)sectionDto.SeccionTipo,
                Orden = sectionDto.Orden
            };
            section.SetDatos(sectionDto.Datos);

            await _unitOfWork.PageSections.AddAsync(section, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(page);
        return Result<PageDto>.Success(dto);
    }

    public static PageDto MapToDto(Page page)
    {
        var sections = page.Sections.OrderBy(s => s.Orden).Select(s => new PageSectionDto
        {
            Id = s.Id,
            PageId = s.PageId,
            SeccionTipo = (int)s.SeccionTipo,
            Orden = s.Orden,
            Datos = s.GetDatos()
        }).ToList();

        return new PageDto
        {
            Id = page.Id,
            Titulo = page.Titulo,
            Slug = page.Slug,
            TipoPlantilla = (int)page.TipoPlantilla,
            MetaDescription = page.MetaDescription,
            Keywords = page.Keywords,
            Orden = page.Orden,
            Activa = page.Activa,
            Sections = sections,
            CreatedAt = page.CreatedAt,
            UpdatedAt = page.UpdatedAt
        };
    }
}
