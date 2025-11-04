using MediatR;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Ofertas.Commands;

public class DeleteOfertaCommandHandler : IRequestHandler<DeleteOfertaCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteOfertaCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteOfertaCommand request, CancellationToken cancellationToken)
    {
        var oferta = await _unitOfWork.Ofertas.GetByIdAsync(request.Id, cancellationToken);

        if (oferta == null)
        {
            throw new NotFoundException("Oferta", request.Id);
        }

        await _unitOfWork.Ofertas.DeleteAsync(oferta, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}

