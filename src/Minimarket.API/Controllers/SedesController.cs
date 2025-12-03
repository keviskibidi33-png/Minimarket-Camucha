using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minimarket.Application.Features.Sedes.Commands;
using Minimarket.Application.Features.Sedes.Queries;
using System;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SedesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SedesController(IMediator mediator)
    {
        _mediator = mediator;
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
                return BadRequest(new { message = result.Error, errors = result.Errors });
            }

            return Ok(result.Data);
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
            if (command == null || command.Sede == null)
            {
                return BadRequest(new { message = "Los datos de la sede son requeridos" });
            }

            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.Error, errors = result.Errors });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
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

