using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Features.Settings.Commands;
using Minimarket.Application.Features.Settings.DTOs;
using Minimarket.Application.Features.Settings.Queries;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class SettingsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(IMediator mediator, ILogger<SettingsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category)
    {
        var query = new GetAllSettingsQuery { Category = category };
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpGet("{key}")]
    [AllowAnonymous] // Permitir acceso público para leer configuraciones (ej: IGV en carrito)
    public async Task<IActionResult> GetByKey(string key)
    {
        var query = new GetSettingByKeyQuery { Key = key };
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpPut("{key}")]
    public async Task<IActionResult> Update(string key, [FromBody] UpdateSystemSettingsDto settingDto)
    {
        if (settingDto == null)
        {
            _logger.LogError("SettingDto es null para key: {Key}", key);
            return BadRequest(new { error = "SettingDto es null" });
        }
        
        var command = new UpdateSystemSettingsCommand
        {
            Setting = new UpdateSystemSettingsDto
            {
                Key = key,
                Value = settingDto.Value ?? string.Empty,
                Description = settingDto.Description,
                IsActive = settingDto.IsActive
            }
        };
        
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            _logger.LogError("Error al actualizar configuración {Key}: {Error}", key, result.Error);
            return BadRequest(result);
        }

        return Ok(result.Data);
    }
}

