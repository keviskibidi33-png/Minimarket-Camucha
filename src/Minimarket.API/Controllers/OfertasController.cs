using System.Collections.Generic;
using System.Linq;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Minimarket.API.Models;
using Minimarket.Application.Features.Ofertas.Commands;
using Minimarket.Application.Features.Ofertas.DTOs;
using Minimarket.Application.Features.Ofertas.Queries;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OfertasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OfertasController> _logger;

    public OfertasController(IMediator mediator, ILogger<OfertasController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous] // Permitir acceso público para la tienda
    public async Task<IActionResult> GetAll([FromQuery] bool? soloActivas)
    {
        try
        {
            var query = new GetAllOfertasQuery { SoloActivas = soloActivas };
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                // Si hay un error, devolver una lista vacía en lugar de un error 500
                // Esto permite que la aplicación funcione incluso si hay problemas con algunas ofertas
                return Ok(new List<OfertaDto>());
            }

            return Ok(result.Data ?? Enumerable.Empty<OfertaDto>());
        }
        catch (Exception ex)
        {
            // En caso de excepción no manejada, devolver una lista vacía
            // Esto permite que la aplicación funcione incluso si hay problemas
            _logger.LogError(ex, "Error inesperado al obtener ofertas");
            return Ok(new List<OfertaDto>());
        }
    }

    [HttpGet("{id}")]
    [AllowAnonymous] // Permitir acceso público para la tienda
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetOfertaByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return NotFound(result);
        }

        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create([FromBody] CreateOfertaRequest? request)
    {
        try
        {
            _logger.LogInformation("Recibida solicitud para crear oferta. Nombre: {Nombre}", request?.Nombre);

            // Validar que el request no sea null
            if (request == null)
            {
                _logger.LogWarning("El request recibido es null");
                return BadRequest(new { message = "El cuerpo de la solicitud no puede estar vacío" });
            }

            // Validar que el nombre no esté vacío
            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                _logger.LogWarning("El nombre de la oferta está vacío");
                return BadRequest(new { message = "El nombre de la oferta es requerido" });
            }

            // Convertir strings a GUIDs
            var categoriasIds = new List<Guid>();
            if (request.CategoriasIds != null && request.CategoriasIds.Any())
            {
                foreach (var idStr in request.CategoriasIds)
                {
                    if (Guid.TryParse(idStr, out Guid guid))
                    {
                        categoriasIds.Add(guid);
                    }
                    else
                    {
                        _logger.LogWarning("ID de categoría inválido: {Id}", idStr);
                    }
                }
            }

            var productosIds = new List<Guid>();
            if (request.ProductosIds != null && request.ProductosIds.Any())
            {
                foreach (var idStr in request.ProductosIds)
                {
                    if (Guid.TryParse(idStr, out Guid guid))
                    {
                        productosIds.Add(guid);
                    }
                    else
                    {
                        _logger.LogWarning("ID de producto inválido: {Id}", idStr);
                    }
                }
            }

            // Crear el DTO con los GUIDs convertidos
            var ofertaDto = new CreateOfertaDto
            {
                Nombre = request.Nombre.Trim(),
                Descripcion = request.Descripcion?.Trim(),
                DescuentoTipo = request.DescuentoTipo,
                DescuentoValor = request.DescuentoValor,
                CategoriasIds = categoriasIds,
                ProductosIds = productosIds,
                FechaInicio = request.FechaInicio,
                FechaFin = request.FechaFin,
                Activa = request.Activa,
                Orden = request.Orden,
                ImagenUrl = request.ImagenUrl?.Trim()
            };

            // Crear el comando desde el DTO
            var command = new CreateOfertaCommand { Oferta = ofertaDto };
            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Error al crear oferta: {Error}", result.Error);
                // Si es un error de validación de negocio, devolver 400
                // Si es un error interno (excepción), devolver 500
                if (result.Error?.Contains("Error al crear") == true || 
                    result.Error?.Contains("Exception") == true ||
                    result.Error?.Contains("excepción") == true)
                {
                    return StatusCode(500, new { message = result.Error ?? "Error interno al crear la oferta", errors = result.Errors });
                }
                return BadRequest(new { message = result.Error, errors = result.Errors });
            }

            _logger.LogInformation("Oferta creada exitosamente. ID: {Id}", result.Data?.Id);
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear oferta. Nombre: {Nombre}", request?.Nombre);
            return StatusCode(500, new { 
                message = "Error interno del servidor al crear la oferta",
                error = ex.Message,
                traceId = System.Diagnostics.Activity.Current?.Id
            });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOfertaRequest request)
    {
        try
        {
            _logger.LogInformation("Recibida solicitud para actualizar oferta. ID: {Id}, Nombre: {Nombre}", id, request?.Nombre);

            // Validar que el request no sea null
            if (request == null)
            {
                _logger.LogWarning("El request recibido es null");
                return BadRequest(new { message = "El cuerpo de la solicitud no puede estar vacío" });
            }

            // Validar que el nombre no esté vacío
            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                _logger.LogWarning("El nombre de la oferta está vacío");
                return BadRequest(new { message = "El nombre de la oferta es requerido" });
            }

            // Convertir strings a GUIDs
            var categoriasIds = new List<Guid>();
            if (request.CategoriasIds != null && request.CategoriasIds.Any())
            {
                foreach (var idStr in request.CategoriasIds)
                {
                    if (Guid.TryParse(idStr, out Guid guid))
                    {
                        categoriasIds.Add(guid);
                    }
                    else
                    {
                        _logger.LogWarning("ID de categoría inválido: {Id}", idStr);
                    }
                }
            }

            var productosIds = new List<Guid>();
            if (request.ProductosIds != null && request.ProductosIds.Any())
            {
                foreach (var idStr in request.ProductosIds)
                {
                    if (Guid.TryParse(idStr, out Guid guid))
                    {
                        productosIds.Add(guid);
                    }
                    else
                    {
                        _logger.LogWarning("ID de producto inválido: {Id}", idStr);
                    }
                }
            }

            // Crear el DTO con los GUIDs convertidos
            var ofertaDto = new UpdateOfertaDto
            {
                Nombre = request.Nombre.Trim(),
                Descripcion = request.Descripcion?.Trim(),
                DescuentoTipo = request.DescuentoTipo,
                DescuentoValor = request.DescuentoValor,
                CategoriasIds = categoriasIds,
                ProductosIds = productosIds,
                FechaInicio = request.FechaInicio,
                FechaFin = request.FechaFin,
                Activa = request.Activa,
                Orden = request.Orden,
                ImagenUrl = request.ImagenUrl?.Trim()
            };

            // Crear el comando desde el DTO
            var command = new UpdateOfertaCommand { Id = id, Oferta = ofertaDto };
            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Error al actualizar oferta: {Error}", result.Error);
                return BadRequest(result);
            }

            _logger.LogInformation("Oferta actualizada exitosamente. ID: {Id}", result.Data?.Id);
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar oferta. ID: {Id}, Nombre: {Nombre}", id, request?.Nombre);
            return StatusCode(500, new { 
                message = "Error interno del servidor al actualizar la oferta",
                error = ex.Message,
                traceId = System.Diagnostics.Activity.Current?.Id
            });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteOfertaCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }
}

