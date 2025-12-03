using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Sedes.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Sedes.Queries;

public class GetAllSedesQueryHandler : IRequestHandler<GetAllSedesQuery, Result<IEnumerable<SedeDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllSedesQueryHandler> _logger;

    public GetAllSedesQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAllSedesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<SedeDto>>> Handle(GetAllSedesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Obteniendo todas las sedes. SoloActivas: {SoloActivas}", request.SoloActivas);

            var sedes = await _unitOfWork.Sedes.GetAllAsync(cancellationToken);
            var sedesList = sedes?.ToList() ?? new List<Domain.Entities.Sede>();
            _logger.LogInformation("Se encontraron {Count} sedes en la base de datos", sedesList.Count);

            var filtered = request.SoloActivas.HasValue && request.SoloActivas.Value
                ? sedesList.Where(s => s.Estado)
                : sedesList;

            // Mapear sedes con manejo de errores individual para cada una
            var result = new List<SedeDto>();
            foreach (var sede in filtered)
            {
                try
                {
                    var dto = MapToDto(sede);
                    result.Add(dto);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al mapear la sede {SedeId} ({SedeNombre}), se omitirá", sede.Id, sede.Nombre);
                    // Continuar con las demás sedes en lugar de fallar completamente
                }
            }

            var orderedResult = result.OrderBy(s => s.Nombre).ToList();
            _logger.LogInformation("Se retornarán {Count} sedes mapeadas correctamente", orderedResult.Count);

            return Result<IEnumerable<SedeDto>>.Success(orderedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico al obtener las sedes");
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
        catch
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

