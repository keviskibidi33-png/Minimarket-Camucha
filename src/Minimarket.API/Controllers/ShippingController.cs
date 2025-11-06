using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minimarket.Application.Features.Shipping.Queries;
using Minimarket.Application.Features.Shipping.Commands;
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

    [HttpGet("rates")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetAllRates([FromQuery] bool? onlyActive)
    {
        var query = new GetAllShippingRatesQuery { OnlyActive = onlyActive };
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpGet("rates/{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetRateById(Guid id)
    {
        var query = new GetShippingRateByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return NotFound(result);
        }

        return Ok(result.Data);
    }

    [HttpPost("rates")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CreateRate([FromBody] CreateShippingRateDto shippingRate)
    {
        var command = new CreateShippingRateCommand { ShippingRate = shippingRate };
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetRateById), new { id = result.Data.Id }, result.Data);
    }

    [HttpPut("rates/{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> UpdateRate(Guid id, [FromBody] UpdateShippingRateDto shippingRate)
    {
        var command = new UpdateShippingRateCommand { Id = id, ShippingRate = shippingRate };
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpDelete("rates/{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteRate(Guid id)
    {
        var command = new DeleteShippingRateCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }
}

