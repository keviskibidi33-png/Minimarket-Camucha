using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minimarket.Application.Features.Banners.Commands;
using Minimarket.Application.Features.Banners.DTOs;
using Minimarket.Application.Features.Banners.Queries;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BannersController : ControllerBase
{
    private readonly IMediator _mediator;

    public BannersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous] // Permitir acceso público para la tienda
    public async Task<IActionResult> GetAll([FromQuery] bool? soloActivos, [FromQuery] int? tipo, [FromQuery] int? posicion)
    {
        var query = new GetAllBannersQuery 
        { 
            SoloActivos = soloActivos,
            Tipo = tipo,
            Posicion = posicion
        };
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    [AllowAnonymous] // Permitir acceso público para la tienda
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetBannerByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return NotFound(result);
        }

        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateBannerCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBannerDto banner)
    {
        var command = new UpdateBannerCommand
        {
            Id = id,
            Banner = banner
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteBannerCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return NoContent();
    }

    [HttpPost("{id}/increment-view")]
    [AllowAnonymous] // Permitir acceso público para tracking
    public async Task<IActionResult> IncrementView(Guid id)
    {
        var command = new IncrementBannerViewCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Endpoint específico para la página de inicio (Home)
    /// Retorna únicamente banners activos, ordenados por prioridad
    /// </summary>
    [HttpGet("home")]
    [AllowAnonymous]
    public async Task<IActionResult> GetHomeBanners()
    {
        var query = new GetAllBannersQuery 
        { 
            SoloActivos = true // Solo banners activos
            // Sin filtros de tipo o posición para mostrar todos los banners de la home
        };
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }
}

