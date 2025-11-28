using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Sedes.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Sedes.Queries;

public class GetAllSedesQueryHandler : IRequestHandler<GetAllSedesQuery, Result<IEnumerable<SedeDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllSedesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<SedeDto>>> Handle(GetAllSedesQuery request, CancellationToken cancellationToken)
    {
        var sedes = await _unitOfWork.Sedes.GetAllAsync(cancellationToken);

        var filtered = request.SoloActivas.HasValue && request.SoloActivas.Value
            ? sedes.Where(s => s.Estado)
            : sedes;

        var result = filtered
            .Select(s => MapToDto(s))
            .OrderBy(s => s.Nombre)
            .ToList();

        return Result<IEnumerable<SedeDto>>.Success(result);
    }

    private static SedeDto MapToDto(Domain.Entities.Sede sede)
    {
        return new SedeDto
        {
            Id = sede.Id,
            Nombre = sede.Nombre,
            Direccion = sede.Direccion,
            Ciudad = sede.Ciudad,
            Pais = sede.Pais,
            Latitud = sede.Latitud,
            Longitud = sede.Longitud,
            Telefono = sede.Telefono,
            Horarios = sede.GetHorarios(),
            LogoUrl = sede.LogoUrl,
            Estado = sede.Estado,
            IsOpen = sede.IsOpen(DateTime.Now),
            NextOpenTime = sede.GetNextOpenTime(),
            GoogleMapsUrl = sede.GoogleMapsUrl
        };
    }
}

