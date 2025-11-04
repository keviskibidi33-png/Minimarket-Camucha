using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minimarket.Application.Features.Shipping.Queries;
using Minimarket.Application.Features.Shipping.DTOs;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShippingController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShippingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("calculate")]
    [AllowAnonymous] // Permitir acceso público para la tienda
    public async Task<IActionResult> CalculateShipping([FromBody] ShippingCalculationRequest request)
    {
        var query = new CalculateShippingQuery { Request = request };
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }
}

