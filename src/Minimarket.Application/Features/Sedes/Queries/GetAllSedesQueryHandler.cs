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
        try
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
        catch (Exception ex)
        {
            return Result<IEnumerable<SedeDto>>.Failure($"Error al obtener las sedes: {ex.Message}");
        }
    }

    private static SedeDto MapToDto(Domain.Entities.Sede sede)
    {
        try
        {
            return new SedeDto
            {
                Id = sede.Id,
                Nombre = sede.Nombre ?? string.Empty,
                Direccion = sede.Direccion ?? string.Empty,
                Ciudad = sede.Ciudad ?? string.Empty,
                Pais = sede.Pais ?? "Perú",
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
        catch (Exception ex)
        {
            // Si hay error al mapear una sede, retornar un DTO básico
            return new SedeDto
            {
                Id = sede.Id,
                Nombre = sede.Nombre ?? "Sede sin nombre",
                Direccion = sede.Direccion ?? string.Empty,
                Ciudad = sede.Ciudad ?? string.Empty,
                Pais = sede.Pais ?? "Perú",
                Latitud = sede.Latitud,
                Longitud = sede.Longitud,
                Telefono = sede.Telefono,
                Horarios = new Dictionary<string, Dictionary<string, string>>(),
                LogoUrl = sede.LogoUrl,
                Estado = sede.Estado,
                IsOpen = false,
                NextOpenTime = null,
                GoogleMapsUrl = sede.GoogleMapsUrl
            };
        }
    }
}

