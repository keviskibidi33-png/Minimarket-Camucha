using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Ofertas.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Ofertas.Commands;

public class CreateOfertaCommandHandler : IRequestHandler<CreateOfertaCommand, Result<OfertaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateOfertaCommandHandler> _logger;

    public CreateOfertaCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateOfertaCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<OfertaDto>> Handle(CreateOfertaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creando oferta. Nombre: {Nombre}, CategoriasIds: {CategoriasCount}, ProductosIds: {ProductosCount}", 
                request.Oferta.Nombre, 
                request.Oferta.CategoriasIds?.Count ?? 0,
                request.Oferta.ProductosIds?.Count ?? 0);

            // Validar que el nombre no esté vacío
            if (string.IsNullOrWhiteSpace(request.Oferta.Nombre))
            {
                throw new BusinessRuleViolationException("El nombre de la oferta es requerido");
            }

            // Validar fechas
            if (request.Oferta.FechaInicio >= request.Oferta.FechaFin)
            {
                throw new BusinessRuleViolationException("La fecha de inicio debe ser anterior a la fecha de fin");
            }

            // Validar que existan categorías/productos si se especifican
            // Asegurar que las listas no sean null
            var categoriasIds = request.Oferta.CategoriasIds ?? new List<Guid>();
            var productosIds = request.Oferta.ProductosIds ?? new List<Guid>();
        
        if (categoriasIds.Any())
        {
            var categorias = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
            var categoriasExistentes = categorias.Where(c => categoriasIds.Contains(c.Id)).ToList();
            if (categoriasExistentes.Count != categoriasIds.Count)
            {
                throw new BusinessRuleViolationException("Una o más categorías especificadas no existen");
            }
        }

        if (productosIds.Any())
        {
            var productos = await _unitOfWork.Products.GetAllAsync(cancellationToken);
            var productosExistentes = productos.Where(p => productosIds.Contains(p.Id)).ToList();
            if (productosExistentes.Count != productosIds.Count)
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

        oferta.SetCategoriasIds(categoriasIds);
        oferta.SetProductosIds(productosIds);

            await _unitOfWork.Ofertas.AddAsync(oferta, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Oferta creada exitosamente. ID: {Id}", oferta.Id);

            var dto = MapToDto(oferta);
            return Result<OfertaDto>.Success(dto);
        }
        catch (BusinessRuleViolationException ex)
        {
            _logger.LogWarning("Error de regla de negocio al crear oferta: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear oferta. Nombre: {Nombre}", request.Oferta.Nombre);
            throw new BusinessRuleViolationException($"Error al crear la oferta: {ex.Message}");
        }
    }

    public static OfertaDto MapToDto(Oferta oferta)
    {
        if (oferta == null)
        {
            throw new ArgumentNullException(nameof(oferta));
        }

        List<Guid> categoriasIds = new List<Guid>();
        List<Guid> productosIds = new List<Guid>();

        try
        {
            categoriasIds = oferta.GetCategoriasIds() ?? new List<Guid>();
        }
        catch (Exception)
        {
            categoriasIds = new List<Guid>();
        }

        try
        {
            productosIds = oferta.GetProductosIds() ?? new List<Guid>();
        }
        catch (Exception)
        {
            productosIds = new List<Guid>();
        }

        try
        {
            return new OfertaDto
            {
                Id = oferta.Id,
                Nombre = oferta.Nombre ?? string.Empty,
                Descripcion = oferta.Descripcion,
                DescuentoTipo = (int)oferta.DescuentoTipo,
                DescuentoValor = oferta.DescuentoValor,
                CategoriasIds = categoriasIds,
                ProductosIds = productosIds,
                FechaInicio = oferta.FechaInicio,
                FechaFin = oferta.FechaFin,
                Activa = oferta.Activa,
                Orden = oferta.Orden,
                ImagenUrl = oferta.ImagenUrl,
                CreatedAt = oferta.CreatedAt,
                UpdatedAt = oferta.UpdatedAt
            };
        }
        catch (Exception)
        {
            // Si hay algún error al mapear, retornar un DTO básico
            return new OfertaDto
            {
                Id = oferta.Id,
                Nombre = oferta.Nombre ?? string.Empty,
                Descripcion = oferta.Descripcion,
                DescuentoTipo = (int)oferta.DescuentoTipo,
                DescuentoValor = oferta.DescuentoValor,
                CategoriasIds = new List<Guid>(),
                ProductosIds = new List<Guid>(),
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
}

