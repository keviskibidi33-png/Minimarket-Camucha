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

        // Las secciones se eliminan automáticamente por cascade delete
        await _unitOfWork.Pages.DeleteAsync(page, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
