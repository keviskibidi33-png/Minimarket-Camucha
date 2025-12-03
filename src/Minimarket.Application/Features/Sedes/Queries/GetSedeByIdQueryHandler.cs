using MediatR;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Sedes.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Sedes.Queries;

public class GetSedeByIdQueryHandler : IRequestHandler<GetSedeByIdQuery, Result<SedeDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSedeByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SedeDto>> Handle(GetSedeByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var sede = await _unitOfWork.Sedes.GetByIdAsync(request.Id, cancellationToken);

            if (sede == null)
            {
                return Result<SedeDto>.Failure($"Sede con ID {request.Id} no encontrada");
            }

            var dto = MapToDto(sede);
            return Result<SedeDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<SedeDto>.Failure($"Error al obtener la sede: {ex.Message}");
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
            // Si hay error al mapear, retornar un DTO básico
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

