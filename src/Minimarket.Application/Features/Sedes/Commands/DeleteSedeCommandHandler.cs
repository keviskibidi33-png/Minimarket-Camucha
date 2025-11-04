using MediatR;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Sedes.Commands;

public class DeleteSedeCommandHandler : IRequestHandler<DeleteSedeCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSedeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteSedeCommand request, CancellationToken cancellationToken)
    {
        var sede = await _unitOfWork.Sedes.GetByIdAsync(request.Id, cancellationToken);

        if (sede == null)
        {
            throw new NotFoundException("Sede", request.Id);
        }

        await _unitOfWork.Sedes.DeleteAsync(sede, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}

