using MediatR;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Ofertas.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Ofertas.Commands;

public class CreateOfertaCommandHandler : IRequestHandler<CreateOfertaCommand, Result<OfertaDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateOfertaCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OfertaDto>> Handle(CreateOfertaCommand request, CancellationToken cancellationToken)
    {
        // Validar fechas
        if (request.Oferta.FechaInicio >= request.Oferta.FechaFin)
        {
            throw new BusinessRuleViolationException("La fecha de inicio debe ser anterior a la fecha de fin");
        }

        // Validar que existan categorías/productos si se especifican
        if (request.Oferta.CategoriasIds.Any())
        {
            var categorias = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
            var categoriasExistentes = categorias.Where(c => request.Oferta.CategoriasIds.Contains(c.Id)).ToList();
            if (categoriasExistentes.Count != request.Oferta.CategoriasIds.Count)
            {
                throw new BusinessRuleViolationException("Una o más categorías especificadas no existen");
            }
        }

        if (request.Oferta.ProductosIds.Any())
        {
            var productos = await _unitOfWork.Products.GetAllAsync(cancellationToken);
            var productosExistentes = productos.Where(p => request.Oferta.ProductosIds.Contains(p.Id)).ToList();
            if (productosExistentes.Count != request.Oferta.ProductosIds.Count)
            {
                throw new BusinessRuleViolationException("Uno o más productos especificados no existen");
            }
        }

        var oferta = new Oferta
        {
            Nombre = request.Oferta.Nombre,
            Descripcion = request.Oferta.Descripcion,
            DescuentoTipo = (DescuentoTipo)request.Oferta.DescuentoTipo,
            DescuentoValor = request.Oferta.DescuentoValor,
            FechaInicio = request.Oferta.FechaInicio,
            FechaFin = request.Oferta.FechaFin,
            Activa = request.Oferta.Activa,
            Orden = request.Oferta.Orden,
            ImagenUrl = request.Oferta.ImagenUrl
        };

        oferta.SetCategoriasIds(request.Oferta.CategoriasIds);
        oferta.SetProductosIds(request.Oferta.ProductosIds);

        await _unitOfWork.Ofertas.AddAsync(oferta, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(oferta);
        return Result<OfertaDto>.Success(dto);
    }

    public static OfertaDto MapToDto(Oferta oferta)
    {
        return new OfertaDto
        {
            Id = oferta.Id,
            Nombre = oferta.Nombre,
            Descripcion = oferta.Descripcion,
            DescuentoTipo = (int)oferta.DescuentoTipo,
            DescuentoValor = oferta.DescuentoValor,
            CategoriasIds = oferta.GetCategoriasIds(),
            ProductosIds = oferta.GetProductosIds(),
            FechaInicio = oferta.FechaInicio,
            FechaFin = oferta.FechaFin,
            Activa = oferta.Activa,
            Orden = oferta.Orden,
            ImagenUrl = oferta.ImagenUrl,
            CreatedAt = oferta.CreatedAt,
            UpdatedAt = oferta.UpdatedAt
        };
    }
}

