using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minimarket.Application.Features.EmailTemplates.Commands;
using Minimarket.Application.Features.EmailTemplates.Queries;
using Minimarket.Domain.Interfaces;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailTemplatesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEmailService _emailService;

    public EmailTemplatesController(IMediator mediator, IEmailService emailService)
    {
        _mediator = mediator;
        _emailService = emailService;
    }

    [HttpGet("{templateType}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetTemplate(string templateType)
    {
        var query = new GetEmailTemplateQuery { TemplateType = templateType };
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpPut("settings")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateEmailTemplateSettingsCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpPost("test/confirmation")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> SendTestConfirmationEmail([FromBody] TestEmailRequest request)
    {
        try
        {
            var result = await _emailService.SendOrderConfirmationAsync(
                request.Email,
                request.CustomerName ?? "Cliente de Prueba",
                request.OrderNumber ?? "TEST-001",
                request.Total ?? 150.00m,
                request.ShippingMethod ?? "delivery",
                request.EstimatedDelivery ?? DateTime.UtcNow.AddDays(3)
            );

            if (result)
            {
                return Ok(new { message = "Correo de confirmación enviado exitosamente", sent = true });
            }

            return BadRequest(new { message = "Error al enviar el correo", sent = false });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error: {ex.Message}", sent = false });
        }
    }

    [HttpPost("test/status-update")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> SendTestStatusUpdateEmail([FromBody] TestStatusEmailRequest request)
    {
        try
        {
            var result = await _emailService.SendOrderStatusUpdateAsync(
                request.Email,
                request.CustomerName ?? "Cliente de Prueba",
                request.OrderNumber ?? "TEST-001",
                request.Status ?? "preparing",
                request.TrackingUrl
            );

            if (result)
            {
                return Ok(new { message = "Correo de actualización de estado enviado exitosamente", sent = true });
            }

            return BadRequest(new { message = "Error al enviar el correo", sent = false });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error: {ex.Message}", sent = false });
        }
    }
}

public class TestEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? OrderNumber { get; set; }
    public decimal? Total { get; set; }
    public string? ShippingMethod { get; set; }
    public DateTime? EstimatedDelivery { get; set; }
}

public class TestStatusEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? OrderNumber { get; set; }
    public string? Status { get; set; }
    public string? TrackingUrl { get; set; }
}
