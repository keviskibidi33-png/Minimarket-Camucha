using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Features.Sedes.Commands;
using Minimarket.Application.Features.Sedes.Queries;
using Minimarket.Application.Features.Sedes.DTOs;
using System;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SedesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SedesController> _logger;

    public SedesController(IMediator mediator, ILogger<SedesController> logger)
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
            var query = new GetAllSedesQuery { SoloActivas = soloActivas };
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                // Si hay un error, pero es un error de procesamiento interno, devolver 500
                // Solo devolver 400 si el request es realmente inválido
                // En este caso, si falla obtener sedes, es un error del servidor, no del cliente
                return StatusCode(500, new { message = result.Error ?? "Error al obtener las sedes", errors = result.Errors });
            }

            // Si result.Data es null, devolver lista vacía en lugar de null
            var sedes = result.Data ?? Array.Empty<SedeDto>();
            return Ok(sedes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor al obtener sedes", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [AllowAnonymous] // Permitir acceso público para la tienda
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetSedeByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return NotFound(result);
        }

        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create([FromBody] CreateSedeCommand command)
    {
        try
        {
            // Validar ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => new { field = x.Key, errors = x.Value?.Errors.Select(e => e.ErrorMessage) })
                    .ToList();
                
                _logger.LogWarning("ModelState inválido al crear sede. Errores: {Errors}", 
                    System.Text.Json.JsonSerializer.Serialize(errors));
                
                return BadRequest(new { 
                    message = "Los datos de la sede son inválidos", 
                    errors = errors 
                });
            }

            if (command == null || command.Sede == null)
            {
                _logger.LogWarning("CreateSedeCommand o Sede es null");
                return BadRequest(new { message = "Los datos de la sede son requeridos" });
            }

            // Validar campos requeridos
            if (string.IsNullOrWhiteSpace(command.Sede.Nombre))
            {
                return BadRequest(new { message = "El nombre de la sede es requerido", field = "nombre" });
            }

            if (string.IsNullOrWhiteSpace(command.Sede.Direccion))
            {
                return BadRequest(new { message = "La dirección de la sede es requerida", field = "direccion" });
            }

            if (string.IsNullOrWhiteSpace(command.Sede.Ciudad))
            {
                return BadRequest(new { message = "La ciudad de la sede es requerida", field = "ciudad" });
            }

            _logger.LogInformation("Creando sede: {Nombre}, Dirección: {Direccion}, Ciudad: {Ciudad}", 
                command.Sede.Nombre, command.Sede.Direccion, command.Sede.Ciudad);

            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                _logger.LogError("Error al crear sede: {Error}", result.Error);
                // Si es un error de validación de negocio, devolver 400
                // Si es un error interno (excepción), devolver 500
                if (result.Error?.Contains("Error al crear") == true || 
                    result.Error?.Contains("Exception") == true ||
                    result.Error?.Contains("excepción") == true)
                {
                    return StatusCode(500, new { message = result.Error ?? "Error interno al crear la sede", errors = result.Errors });
                }
                return BadRequest(new { message = result.Error, errors = result.Errors });
            }

            _logger.LogInformation("Sede creada exitosamente: {SedeId}", result.Data?.Id);
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al crear sede");
            return StatusCode(500, new { message = "Error interno del servidor al crear sede", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSedeCommand command)
    {
        try
        {
            if (command == null)
            {
                return BadRequest(new { message = "El comando no puede ser nulo" });
            }

            command.Id = id;
            
            if (command.Sede == null)
            {
                return BadRequest(new { message = "Los datos de la sede son requeridos" });
            }

            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error al actualizar la sede: {ex.Message}", details = ex.ToString() });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var command = new DeleteSedeCommand { Id = id };
            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.Error, errors = result.Errors });
            }

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor al eliminar sede", error = ex.Message });
        }
    }
}

