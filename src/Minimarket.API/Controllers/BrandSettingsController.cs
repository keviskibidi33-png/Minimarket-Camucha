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
        var query = new GetBrandSettingsQuery();
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpPut]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Update([FromBody] UpdateBrandSettingsCommand command)
    {
        // Obtener ID del usuario actual desde el token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            command.UpdatedBy = userId;
        }

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }
}

