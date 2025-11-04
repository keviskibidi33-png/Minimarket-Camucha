using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minimarket.Application.Features.Translations.Commands;
using Minimarket.Application.Features.Translations.Queries;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TranslationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TranslationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{languageCode}")]
    [AllowAnonymous] // Permitir acceso público para la tienda
    public async Task<IActionResult> GetByLanguage(string languageCode, [FromQuery] string? category)
    {
        var query = new GetTranslationsByLanguageQuery 
        { 
            LanguageCode = languageCode,
            Category = category
        };
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create([FromBody] CreateTranslationCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpPost("bulk")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> BulkCreate([FromBody] BulkCreateTranslationsCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(new { Created = result.Data });
    }
}
