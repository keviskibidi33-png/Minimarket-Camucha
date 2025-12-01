using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Minimarket.Application.Features.Orders.Commands;
using Minimarket.Application.Features.Orders.DTOs;
using Minimarket.Application.Features.Orders.Queries;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IMediator mediator, 
        UserManager<IdentityUser<Guid>> userManager,
        ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _userManager = userManager;
        _logger = logger;
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

    /// <summary>
    /// Obtiene los detalles de un pedido por su ID
    /// </summary>
    /// <param name="id">ID del pedido</param>
    /// <returns>Detalles del pedido si pertenece al usuario autenticado</returns>
    [HttpGet("{id}")]
    [Authorize] // Requiere autenticación para ver detalles del pedido
    // Removido [Produces] para evitar problemas con formatters - usamos ContentResult directamente
    [ProducesResponseType(typeof(WebOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        try
        {
            _logger.LogInformation("GetOrderById called with orderId: {OrderId}", id);
            
            // Validar que el ID no sea vacío
            if (id == Guid.Empty)
            {
                _logger.LogWarning("Invalid order ID provided: {OrderId}", id);
                return BadRequest(new { message = "ID de pedido inválido" });
            }

            // Obtener el ID del usuario autenticado desde el token JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("User not authenticated for order {OrderId}", id);
                return Unauthorized(new { 
                    succeeded = false,
                    message = "Usuario no autenticado" 
                });
            }                                                                                                                                                                                                                                                                                                   

            // Obtener el usuario y su email
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                _logger.LogWarning("User not found for userId: {UserId}", userId);
                return Unauthorized(new { 
                    succeeded = false,
                    message = "Usuario no encontrado" 
                });
            }

            _logger.LogInformation("Fetching order {OrderId} for user {Email}", id, user.Email);

            // Ejecutar la query
            var query = new GetOrderByIdQuery { OrderId = id };
            var result = await _mediator.Send(query);

            _logger.LogInformation(
                "Query result - Succeeded: {Succeeded}, HasData: {HasData}, Error: {Error}", 
                result.Succeeded, 
                result.Data != null, 
                result.Error);

            // Validar resultado de la query
            if (!result.Succeeded)
            {
                _logger.LogWarning("Order {OrderId} not found or query failed: {Error}", id, result.Error);
                return NotFound(new { 
                    succeeded = false,
                    message = result.Error ?? "Pedido no encontrado"
                });
            }

            // Validar que los datos no sean null
            if (result.Data == null)
            {
                _logger.LogError("Order {OrderId} query succeeded but Data is null!", id);
                return StatusCode(
                    StatusCodes.Status500InternalServerError, 
                    new { 
                        succeeded = false,
                        message = "Error interno: datos del pedido no disponibles" 
                    });
            }

            _logger.LogInformation(
                "Order data retrieved - OrderNumber: {OrderNumber}, CustomerEmail: {CustomerEmail}, UserEmail: {UserEmail}", 
                result.Data.OrderNumber, 
                result.Data.CustomerEmail, 
                user.Email);

            // Verificar que el pedido pertenece al usuario autenticado
            _logger.LogInformation(
                "Validating order ownership - OrderEmail: '{OrderEmail}', UserEmail: '{UserEmail}', EmailsMatch: {Match}",
                result.Data.CustomerEmail ?? "NULL",
                user.Email ?? "NULL",
                !string.IsNullOrEmpty(result.Data.CustomerEmail) && 
                result.Data.CustomerEmail.Equals(user.Email, StringComparison.OrdinalIgnoreCase));
            
            if (string.IsNullOrEmpty(result.Data.CustomerEmail))
            {
                _logger.LogWarning(
                    "Order {OrderId} has null or empty CustomerEmail. UserEmail: {UserEmail}", 
                    id, 
                    user.Email);
                return StatusCode(
                    StatusCodes.Status403Forbidden, 
                    new { 
                        succeeded = false,
                        message = "El pedido no tiene un email de cliente válido" 
                    });
            }
            
            if (!result.Data.CustomerEmail.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Order {OrderId} does not belong to user {Email}. Order belongs to {OrderEmail}. Comparison: '{OrderEmail}' != '{UserEmail}'", 
                    id, 
                    user.Email, 
                    result.Data.CustomerEmail,
                    result.Data.CustomerEmail,
                    user.Email);
                return StatusCode(
                    StatusCodes.Status403Forbidden, 
                    new { 
                        succeeded = false,
                        message = "No tienes permiso para ver este pedido" 
                    });
            }
            
            _logger.LogInformation("Order ownership validated successfully for order {OrderId}", id);

            _logger.LogInformation(
                "Successfully retrieved order {OrderId} for user {Email}. Returning data with {ItemCount} items", 
                id, 
                user.Email, 
                result.Data.Items?.Count ?? 0);
            
            // Validar que el DTO tenga datos mínimos antes de serializar
            if (result.Data.Id == Guid.Empty)
            {
                _logger.LogError("Order {OrderId} has invalid ID in DTO", id);
                return StatusCode(
                    StatusCodes.Status500InternalServerError, 
                    new { 
                        succeeded = false,
                        message = "Error interno: datos del pedido inválidos" 
                    });
            }
            
            // Log detallado antes de serializar
            _logger.LogInformation(
                "About to return Ok() with DTO - Id: {Id}, OrderNumber: {OrderNumber}, ItemsCount: {Count}, Total: {Total}",
                result.Data.Id,
                result.Data.OrderNumber,
                result.Data.Items?.Count ?? 0,
                result.Data.Total);
            
            // Loggear detalles del DTO antes de devolver
            _logger.LogInformation(
                "Final DTO check - Id: {Id}, OrderNumber: {OrderNumber}, CustomerEmail: {Email}, Items: {Items}, Total: {Total}, Status: {Status}",
                result.Data.Id,
                result.Data.OrderNumber,
                result.Data.CustomerEmail,
                result.Data.Items?.Count ?? 0,
                result.Data.Total,
                result.Data.Status);
            
            // Serializar manualmente y devolver como ContentResult para evitar problemas con formatters
            // Esta es la forma más segura de asegurar que la respuesta se serialice correctamente
            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };
            
            var json = System.Text.Json.JsonSerializer.Serialize(result.Data, jsonOptions);
            
            _logger.LogInformation(
                "Manual serialization successful for order {OrderId}. JSON length: {Length}", 
                id, 
                json.Length);
            
            return new ContentResult
            {
                Content = json,
                ContentType = "application/json",
                StatusCode = StatusCodes.Status200OK
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", id);
            return StatusCode(
                StatusCodes.Status500InternalServerError, 
                new { 
                    succeeded = false,
                    message = "Error interno del servidor", 
                    error = _logger.IsEnabled(LogLevel.Debug) ? ex.Message : "Error al procesar la solicitud"
                });
        }
    }

    [HttpGet("my-orders")]
    [Authorize] // Requiere autenticación
    public async Task<IActionResult> GetMyOrders()
    {
        // Obtener el ID del usuario autenticado desde el token JWT
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        // Obtener el usuario y su email
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || string.IsNullOrEmpty(user.Email))
        {
            return Unauthorized();
        }

        var query = new GetUserOrdersQuery { UserEmail = user.Email };
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpGet("admin/all")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] string? status,
        [FromQuery] string? searchTerm,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetAllOrdersQuery
        {
            Status = status,
            SearchTerm = searchTerm,
            StartDate = startDate,
            EndDate = endDate,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var command = new UpdateOrderStatusCommand
        {
            OrderId = id,
            Status = request.Status,
            TrackingUrl = request.TrackingUrl,
            EstimatedDelivery = request.EstimatedDelivery
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(new { success = true, message = "Estado del pedido actualizado correctamente" });
    }

    [HttpPut("{id}/payment-proof")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> UpdatePaymentProof(Guid id, [FromBody] UpdatePaymentProofRequest request)
    {
        var command = new UpdatePaymentProofCommand
        {
            OrderId = id,
            PaymentProofUrl = request.PaymentProofUrl
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(new { success = true, message = "Comprobante de pago actualizado correctamente" });
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ApproveOrder(Guid id, [FromBody] ApproveOrderRequest request)
    {
        var command = new ApproveOrderCommand
        {
            OrderId = id,
            SendPaymentVerifiedEmail = request?.SendPaymentVerifiedEmail ?? false
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(new { success = true, message = "Pedido aprobado correctamente" });
    }

    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> RejectOrder(Guid id, [FromBody] RejectOrderRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest(new { error = "El motivo del rechazo es obligatorio" });
        }

        var command = new RejectOrderCommand
        {
            OrderId = id,
            Reason = request.Reason
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(new { success = true, message = "Pedido rechazado correctamente" });
    }
}

public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? TrackingUrl { get; set; }
    public DateTime? EstimatedDelivery { get; set; }
}

public class UpdatePaymentProofRequest
{
    public string PaymentProofUrl { get; set; } = string.Empty;
}

public class ApproveOrderRequest
{
    public bool SendPaymentVerifiedEmail { get; set; } = false;
}

public class RejectOrderRequest
{
    public string Reason { get; set; } = string.Empty;
}

