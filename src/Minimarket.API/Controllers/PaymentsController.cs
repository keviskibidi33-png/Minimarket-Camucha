using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minimarket.Application.Features.Payments.Commands;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("create-intent")]
    [AllowAnonymous] // Permitir acceso público para la tienda
    public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpPost("confirm")]
    [Authorize] // Requiere autenticación para confirmar pago
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }
}

