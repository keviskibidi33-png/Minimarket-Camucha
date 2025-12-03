using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minimarket.Application.Features.BrandSettings.Commands;
using Minimarket.Application.Features.BrandSettings.Queries;
using System.Security.Claims;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrandSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BrandSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous] // Permitir acceso público para que la tienda pueda cargar los colores
    public async Task<IActionResult> Get()
    {
        try
        {
            var query = new GetBrandSettingsQuery();
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.Error, errors = result.Errors });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor al obtener configuración de marca", error = ex.Message });
        }
    }

    [HttpPut]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Update([FromBody] UpdateBrandSettingsCommand command)
    {
        try
        {
            if (command == null)
            {
                return BadRequest(new { message = "Los datos de configuración son requeridos" });
            }

            // Obtener ID del usuario actual desde el token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                command.UpdatedBy = userId;
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
            return StatusCode(500, new { message = "Error interno del servidor al actualizar configuración de marca", error = ex.Message });
        }
    }
}

