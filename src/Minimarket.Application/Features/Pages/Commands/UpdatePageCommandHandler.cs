using MediatR;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Pages.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Pages.Commands;

public class UpdatePageCommandHandler : IRequestHandler<UpdatePageCommand, Result<PageDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePageCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PageDto>> Handle(UpdatePageCommand request, CancellationToken cancellationToken)
    {
        var page = await _unitOfWork.Pages.GetByIdAsync(request.Id, cancellationToken);

        if (page == null)
        {
            throw new NotFoundException("Page", request.Id);
        }

        // Verificar slug único (excepto la página actual)
        var existingPages = await _unitOfWork.Pages.GetAllAsync(cancellationToken);
        if (existingPages.Any(p => p.Slug == request.Page.Slug && p.Id != request.Id))
        {
            throw new BusinessRuleViolationException($"Ya existe otra página con el slug '{request.Page.Slug}'");
        }

        page.Titulo = request.Page.Titulo;
        page.Slug = request.Page.Slug;
        page.TipoPlantilla = (TipoPlantilla)request.Page.TipoPlantilla;
        page.MetaDescription = request.Page.MetaDescription;
        page.Keywords = request.Page.Keywords;
        page.Orden = request.Page.Orden;
        page.Activa = request.Page.Activa;
        page.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Pages.UpdateAsync(page, cancellationToken);

        // Obtener secciones existentes
        var existingSections = (await _unitOfWork.PageSections.GetAllAsync(cancellationToken))
            .Where(s => s.PageId == page.Id)
            .ToList();

        // Eliminar secciones que no están en la lista
        var newSectionIds = request.Page.Sections
            .Where(s => s.Id.HasValue)
            .Select(s => s.Id!.Value)
            .ToHashSet();

        var sectionsToDelete = existingSections.Where(s => !newSectionIds.Contains(s.Id)).ToList();
        foreach (var section in sectionsToDelete)
        {
            await _unitOfWork.PageSections.DeleteAsync(section, cancellationToken);
        }

        // Actualizar o crear secciones
        foreach (var sectionDto in request.Page.Sections.OrderBy(s => s.Orden))
        {
            if (sectionDto.Id.HasValue)
            {
                // Actualizar sección existente
                var section = existingSections.FirstOrDefault(s => s.Id == sectionDto.Id!.Value);
                if (section != null)
                {
                    section.SeccionTipo = (SeccionTipo)sectionDto.SeccionTipo;
                    section.Orden = sectionDto.Orden;
                    section.SetDatos(sectionDto.Datos);
                    await _unitOfWork.PageSections.UpdateAsync(section, cancellationToken);
                }
            }
            else
            {
                // Crear nueva sección
                var newSection = new PageSection
                {
                    PageId = page.Id,
                    SeccionTipo = (SeccionTipo)sectionDto.SeccionTipo,
                    Orden = sectionDto.Orden
                };
                newSection.SetDatos(sectionDto.Datos);
                await _unitOfWork.PageSections.AddAsync(newSection, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = CreatePageCommandHandler.MapToDto(page);
        return Result<PageDto>.Success(dto);
    }
}
