using MediatR;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Ofertas.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Ofertas.Commands;

public class UpdateOfertaCommandHandler : IRequestHandler<UpdateOfertaCommand, Result<OfertaDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOfertaCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OfertaDto>> Handle(UpdateOfertaCommand request, CancellationToken cancellationToken)
    {
        var oferta = await _unitOfWork.Ofertas.GetByIdAsync(request.Id, cancellationToken);

        if (oferta == null)
        {
            throw new NotFoundException("Oferta", request.Id);
        }

        // Validar fechas
        if (request.Oferta.FechaInicio >= request.Oferta.FechaFin)
        {
            throw new BusinessRuleViolationException("La fecha de inicio debe ser anterior a la fecha de fin");
        }

        oferta.Nombre = request.Oferta.Nombre;
        oferta.Descripcion = request.Oferta.Descripcion;
        oferta.DescuentoTipo = (DescuentoTipo)request.Oferta.DescuentoTipo;
        oferta.DescuentoValor = request.Oferta.DescuentoValor;
        oferta.FechaInicio = request.Oferta.FechaInicio;
        oferta.FechaFin = request.Oferta.FechaFin;
        oferta.Activa = request.Oferta.Activa;
        oferta.Orden = request.Oferta.Orden;
        oferta.SetCategoriasIds(request.Oferta.CategoriasIds);
        oferta.SetProductosIds(request.Oferta.ProductosIds);
        oferta.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Ofertas.UpdateAsync(oferta, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = CreateOfertaCommandHandler.MapToDto(oferta);
        return Result<OfertaDto>.Success(dto);
    }
}

