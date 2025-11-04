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
        var sede = await _unitOfWork.Sedes.GetByIdAsync(request.Id, cancellationToken);

        if (sede == null)
        {
            throw new NotFoundException("Sede", request.Id);
        }

        var dto = MapToDto(sede);
        return Result<SedeDto>.Success(dto);
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
            NextOpenTime = sede.GetNextOpenTime()
        };
    }
}

