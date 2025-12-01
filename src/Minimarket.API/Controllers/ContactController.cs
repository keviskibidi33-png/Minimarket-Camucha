using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minimarket.Domain.Interfaces;
using System.Net;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ContactController> _logger;

    public ContactController(
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<ContactController> logger)
    {
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> SendContactEmail([FromBody] ContactEmailRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name) || 
                string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { message = "Nombre, email y mensaje son requeridos" });
            }

            // Validar email
            if (!System.Text.RegularExpressions.Regex.IsMatch(request.Email, 
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return BadRequest(new { message = "Email inválido" });
            }

            // Obtener email de destino - siempre usar minimarket.camucha@gmail.com para contactos
            var emailSettings = _configuration.GetSection("EmailSettings");
            var toEmail = "minimarket.camucha@gmail.com"; // Correo fijo para todos los contactos
            var fromName = emailSettings["FromName"] ?? "Minimarket Camucha";

            // Construir el cuerpo del email
            var subject = !string.IsNullOrWhiteSpace(request.Subject) 
                ? $"Contacto: {request.Subject}" 
                : "Nuevo mensaje de contacto";

            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <h2 style='color: #4A90E2;'>Nuevo mensaje de contacto</h2>
                    <div style='background-color: #f5f5f5; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Nombre:</strong> {WebUtility.HtmlEncode(request.Name)}</p>
                        <p><strong>Email:</strong> {WebUtility.HtmlEncode(request.Email)}</p>
                        {(string.IsNullOrWhiteSpace(request.Phone) ? "" : $"<p><strong>Teléfono:</strong> {WebUtility.HtmlEncode(request.Phone)}</p>")}
                        {(string.IsNullOrWhiteSpace(request.Subject) ? "" : $"<p><strong>Asunto:</strong> {WebUtility.HtmlEncode(request.Subject)}</p>")}
                        <p><strong>Mensaje:</strong></p>
                        <p style='white-space: pre-wrap;'>{WebUtility.HtmlEncode(request.Message)}</p>
                    </div>
                    <p style='color: #666; font-size: 12px;'>Este mensaje fue enviado desde el formulario de contacto del sitio web.</p>
                </body>
                </html>";

            // Enviar email
            var emailSent = await _emailService.SendEmailAsync(
                to: toEmail,
                subject: subject,
                body: body
            );

            if (emailSent)
            {
                _logger.LogInformation("Contact email sent successfully from {Email} to {ToEmail}", 
                    request.Email, toEmail);
                return Ok(new { message = "Mensaje enviado exitosamente" });
            }
            else
            {
                _logger.LogError("Failed to send contact email from {Email} to {ToEmail}", 
                    request.Email, toEmail);
                return StatusCode(500, new { message = "Error al enviar el mensaje" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing contact email request");
            return StatusCode(500, new { message = "Error al procesar la solicitud" });
        }
    }
}

public class ContactEmailRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Subject { get; set; }
    public string Message { get; set; } = string.Empty;
}

