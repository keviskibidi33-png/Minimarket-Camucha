using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Features.Pages.Commands;
using Minimarket.Application.Features.Pages.Queries;
using Minimarket.Application.Features.Pages.DTOs;
using System.Linq;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PagesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PagesController> _logger;

    public PagesController(IMediator mediator, ILogger<PagesController> logger)
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
            var query = new GetAllPagesQuery { SoloActivas = soloActivas };
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                // En lugar de BadRequest, devolver lista vacía para evitar errores 500 en frontend
                _logger.LogWarning("Error al obtener páginas: {Error}", result.Error);
                return Ok(result.Data ?? Enumerable.Empty<PageDto>());
            }

            return Ok(result.Data ?? Enumerable.Empty<PageDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al obtener páginas");
            // Devolver lista vacía en lugar de 500 para evitar errores en frontend
            return Ok(Enumerable.Empty<PageDto>());
        }
    }

    [HttpGet("{slug}")]
    [AllowAnonymous] // Permitir acceso público para la tienda
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var query = new GetPageBySlugQuery { Slug = slug };
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return NotFound(result);
        }

        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create([FromBody] CreatePageCommand command)
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePageCommand command)
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
        var command = new DeletePageCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }
}
