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
        var sede = new Sede
        {
            Nombre = request.Sede.Nombre,
            Direccion = request.Sede.Direccion,
            Ciudad = request.Sede.Ciudad,
            Pais = request.Sede.Pais,
            Latitud = request.Sede.Latitud,
            Longitud = request.Sede.Longitud,
            Telefono = request.Sede.Telefono,
            LogoUrl = request.Sede.LogoUrl,
            Estado = request.Sede.Estado,
            GoogleMapsUrl = request.Sede.GoogleMapsUrl
        };

        sede.SetHorarios(request.Sede.Horarios);

        await _unitOfWork.Sedes.AddAsync(sede, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(sede);
        return Result<SedeDto>.Success(dto);
    }

    public static SedeDto MapToDto(Domain.Entities.Sede sede)
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

