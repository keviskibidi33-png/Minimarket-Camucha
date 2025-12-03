using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Sedes.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Sedes.Commands;

public class CreateSedeCommandHandler : IRequestHandler<CreateSedeCommand, Result<SedeDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateSedeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SedeDto>> Handle(CreateSedeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.Sede == null)
            {
                return Result<SedeDto>.Failure("Los datos de la sede son requeridos");
            }

            var sede = new Sede
            {
                Nombre = request.Sede.Nombre ?? string.Empty,
                Direccion = request.Sede.Direccion ?? string.Empty,
                Ciudad = request.Sede.Ciudad ?? string.Empty,
                Pais = request.Sede.Pais ?? "Perú",
                Latitud = request.Sede.Latitud,
                Longitud = request.Sede.Longitud,
                Telefono = request.Sede.Telefono,
                LogoUrl = request.Sede.LogoUrl,
                Estado = request.Sede.Estado,
                GoogleMapsUrl = request.Sede.GoogleMapsUrl
            };

            sede.SetHorarios(request.Sede.Horarios ?? new Dictionary<string, Dictionary<string, string>>());

            await _unitOfWork.Sedes.AddAsync(sede, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = MapToDto(sede);
            return Result<SedeDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<SedeDto>.Failure($"Error al crear la sede: {ex.Message}");
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

