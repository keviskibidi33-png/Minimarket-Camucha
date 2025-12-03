using MediatR;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Sedes.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Sedes.Commands;

public class UpdateSedeCommandHandler : IRequestHandler<UpdateSedeCommand, Result<SedeDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSedeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SedeDto>> Handle(UpdateSedeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var sede = await _unitOfWork.Sedes.GetByIdAsync(request.Id, cancellationToken);

            if (sede == null)
            {
                return Result<SedeDto>.Failure($"Sede con ID {request.Id} no encontrada");
            }

            if (request.Sede == null)
            {
                return Result<SedeDto>.Failure("Los datos de la sede son requeridos");
            }

            sede.Nombre = request.Sede.Nombre ?? sede.Nombre;
            sede.Direccion = request.Sede.Direccion ?? sede.Direccion;
            sede.Ciudad = request.Sede.Ciudad ?? sede.Ciudad;
            sede.Pais = request.Sede.Pais ?? sede.Pais;
            sede.Latitud = request.Sede.Latitud;
            sede.Longitud = request.Sede.Longitud;
            sede.Telefono = request.Sede.Telefono;
            sede.LogoUrl = request.Sede.LogoUrl;
            sede.Estado = request.Sede.Estado;
            sede.GoogleMapsUrl = request.Sede.GoogleMapsUrl;
            
            // Validar y establecer horarios
            if (request.Sede.Horarios != null && request.Sede.Horarios.Count > 0)
            {
                sede.SetHorarios(request.Sede.Horarios);
            }
            // Si no hay horarios, mantener los existentes (no sobrescribir)
            
            sede.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Sedes.UpdateAsync(sede, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = MapToDto(sede);
            return Result<SedeDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<SedeDto>.Failure($"Error al actualizar la sede: {ex.Message}");
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
        catch (Exception)
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

