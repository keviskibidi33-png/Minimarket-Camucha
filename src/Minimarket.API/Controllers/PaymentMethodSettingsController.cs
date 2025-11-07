using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minimarket.Application.Features.PaymentMethods.Commands;
using Minimarket.Application.Features.PaymentMethods.Queries;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentMethodSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentMethodSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous] // Permitir acceso público para obtener métodos habilitados (para el carrito)
    public async Task<IActionResult> GetAll([FromQuery] bool enabledOnly = false)
    {
        var query = new GetPaymentMethodSettingsQuery();
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(new { 
                succeeded = false,
                errors = result.Errors,
                message = result.Error
            });
        }

        var data = result.Data;
        if (enabledOnly)
        {
            data = data.Where(pms => pms.IsEnabled).ToList();
        }

        return Ok(data);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaymentMethodSettingsCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new { 
                succeeded = false,
                errors = result.Errors,
                message = result.Error
            });
        }

        return Ok(result.Data);
    }
}

