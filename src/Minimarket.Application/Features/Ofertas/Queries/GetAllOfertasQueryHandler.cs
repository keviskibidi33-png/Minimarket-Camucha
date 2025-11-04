using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Ofertas.DTOs;
using Minimarket.Application.Features.Ofertas.Commands;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Ofertas.Queries;

public class GetAllOfertasQueryHandler : IRequestHandler<GetAllOfertasQuery, Result<IEnumerable<OfertaDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllOfertasQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<OfertaDto>>> Handle(GetAllOfertasQuery request, CancellationToken cancellationToken)
    {
        var ofertas = await _unitOfWork.Ofertas.GetAllAsync(cancellationToken);

        var filtered = request.SoloActivas.HasValue && request.SoloActivas.Value
            ? ofertas.Where(o => o.Activa && o.IsActive(DateTime.Now))
            : ofertas;

        var result = filtered
            .OrderBy(o => o.Orden)
            .ThenBy(o => o.FechaInicio)
            .Select(o => CreateOfertaCommandHandler.MapToDto(o))
            .ToList();

        return Result<IEnumerable<OfertaDto>>.Success(result);
    }
}

