using MediatR;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Ofertas.DTOs;
using Minimarket.Application.Features.Ofertas.Commands;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Ofertas.Queries;

public class GetOfertaByIdQueryHandler : IRequestHandler<GetOfertaByIdQuery, Result<OfertaDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetOfertaByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OfertaDto>> Handle(GetOfertaByIdQuery request, CancellationToken cancellationToken)
    {
        var oferta = await _unitOfWork.Ofertas.GetByIdAsync(request.Id, cancellationToken);

        if (oferta == null)
        {
            throw new NotFoundException("Oferta", request.Id);
        }

        var dto = CreateOfertaCommandHandler.MapToDto(oferta);
        return Result<OfertaDto>.Success(dto);
    }
}

