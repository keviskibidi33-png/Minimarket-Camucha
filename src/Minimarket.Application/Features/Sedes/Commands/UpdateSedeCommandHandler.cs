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
        var sede = await _unitOfWork.Sedes.GetByIdAsync(request.Id, cancellationToken);

        if (sede == null)
        {
            throw new NotFoundException("Sede", request.Id);
        }

        sede.Nombre = request.Sede.Nombre;
        sede.Direccion = request.Sede.Direccion;
        sede.Ciudad = request.Sede.Ciudad;
        sede.Pais = request.Sede.Pais;
        sede.Latitud = request.Sede.Latitud;
        sede.Longitud = request.Sede.Longitud;
        sede.Telefono = request.Sede.Telefono;
        sede.LogoUrl = request.Sede.LogoUrl;
        sede.Estado = request.Sede.Estado;
        sede.GoogleMapsUrl = request.Sede.GoogleMapsUrl;
        sede.SetHorarios(request.Sede.Horarios);
        sede.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Sedes.UpdateAsync(sede, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = CreateSedeCommandHandler.MapToDto(sede);
        return Result<SedeDto>.Success(dto);
    }
}

