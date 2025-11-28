using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minimarket.Application.Features.DocumentSettings.Commands;
using Minimarket.Application.Features.DocumentSettings.Queries;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("view-settings")]
    [AllowAnonymous] // Permitir acceso público para obtener configuración
    public async Task<IActionResult> GetViewSettings()
    {
        var query = new GetDocumentViewSettingsQuery();
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpPut("view-settings")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> UpdateViewSettings([FromBody] UpdateDocumentViewSettingsCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }
}

