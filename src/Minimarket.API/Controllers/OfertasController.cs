using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minimarket.Application.Features.Ofertas.Commands;
using Minimarket.Application.Features.Ofertas.Queries;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OfertasController : ControllerBase
{
    private readonly IMediator _mediator;

    public OfertasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous] // Permitir acceso público para la tienda
    public async Task<IActionResult> GetAll([FromQuery] bool? soloActivas)
    {
        var query = new GetAllOfertasQuery { SoloActivas = soloActivas };
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
    public async Task<IActionResult> Create([FromBody] CreateOfertaCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOfertaCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
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

