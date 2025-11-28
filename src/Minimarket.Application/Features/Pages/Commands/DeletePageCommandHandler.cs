using MediatR;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Pages.Commands;

public class DeletePageCommandHandler : IRequestHandler<DeletePageCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeletePageCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeletePageCommand request, CancellationToken cancellationToken)
    {
        var page = await _unitOfWork.Pages.GetByIdAsync(request.Id, cancellationToken);

        if (page == null)
        {
            throw new NotFoundException("Page", request.Id);
        }

        // Validar que siempre haya al menos una noticia activa
        var allPages = await _unitOfWork.Pages.GetAllAsync(cancellationToken);
        var activePages = allPages.Where(p => p.Activa && p.Id != request.Id).ToList();
        
        if (page.Activa && activePages.Count == 0)
        {
            throw new BusinessRuleViolationException("No se puede eliminar la última noticia activa. Debe haber al menos una noticia activa en el sistema.");
        }

        // Eliminar todas las secciones asociadas manualmente
        var sections = await _unitOfWork.PageSections.FindAsync(ps => ps.PageId == request.Id, cancellationToken);
        foreach (var section in sections)
        {
            await _unitOfWork.PageSections.DeleteAsync(section, cancellationToken);
        }

        // Eliminar la página
        await _unitOfWork.Pages.DeleteAsync(page, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
