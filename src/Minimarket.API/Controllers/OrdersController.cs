using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minimarket.Application.Features.Orders.Commands;
using Minimarket.Application.Features.Orders.DTOs;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [AllowAnonymous] // Permitir acceso público para checkout
    public async Task<IActionResult> CreateOrder([FromBody] CreateWebOrderDto order)
    {
        var command = new CreateWebOrderCommand { Order = order };
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetOrderById), new { id = result.Data.Id }, result.Data);
    }

    [HttpGet("{id}")]
    [AllowAnonymous] // Permitir acceso público para consultar pedido
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        // TODO: Implementar query para obtener pedido por ID
        return Ok();
    }
}

