using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minimarket.Domain.Interfaces;
using MimeKit;
using System.Net.Http.Json;
using Microsoft.Extensions.Hosting;

namespace Minimarket.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly IHostEnvironment _hostEnvironment;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger, IHostEnvironment hostEnvironment)
    {
        _configuration = configuration;
        _logger = logger;
        _hostEnvironment = hostEnvironment;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, string? attachmentPath = null, string? attachmentName = null)
    {
        try
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var smtpServer = emailSettings["SmtpServer"];
            var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
            var smtpUser = emailSettings["SmtpUser"];
            var smtpPassword = emailSettings["SmtpPassword"];
            var fromEmail = emailSettings["FromEmail"];
            var fromName = emailSettings["FromName"] ?? "Minimarket Camucha";

            // Si no hay configuración SMTP, usar API REST externa (Resend)
            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUser))
            {
                return await SendEmailViaApiAsync(to, subject, body, attachmentPath, attachmentName);
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };

            if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath))
            {
                bodyBuilder.Attachments.Add(attachmentPath);
            }

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {Email}", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}", to);
            return false;
        }
    }

    public async Task<bool> SendSaleReceiptAsync(string toEmail, string customerName, string saleNumber, string pdfPath, string documentType)
    {
        var documentTypeText = documentType == "Factura" ? "Factura" : "Boleta";
        var subject = $"{documentTypeText} {saleNumber} - Minimarket Camucha";
        
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2 style='color: #2563eb;'>Minimarket Camucha</h2>
                <p>Estimado/a {customerName},</p>
                <p>Adjunto encontrará su {documentTypeText.ToLower()} de venta número <strong>{saleNumber}</strong>.</p>
                <p>Gracias por su compra.</p>
                <hr>
                <p style='color: #666; font-size: 12px;'>Este es un correo automático, por favor no responder.</p>
            </body>
            </html>";

        return await SendEmailAsync(toEmail, subject, body, pdfPath, $"{documentTypeText}_{saleNumber}.pdf");
    }

    public async Task<bool> SendOrderConfirmationAsync(string toEmail, string customerName, string orderNumber, decimal total, string shippingMethod, DateTime? estimatedDelivery)
    {
        var shippingMethodText = shippingMethod == "delivery" ? "Despacho a Domicilio" : "Retiro en Tienda";
        var deliveryText = estimatedDelivery.HasValue 
            ? $"Tu pedido será entregado aproximadamente el {estimatedDelivery.Value:dd 'de' MMMM, yyyy}"
            : "Te notificaremos cuando tu pedido esté listo.";
        
        var subject = $"Confirmación de Pedido #{orderNumber} - Minimarket Camucha";
        
        // Cargar imágenes desde el sistema de archivos e incrustarlas
        var wwwrootPath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", "email-templates");
        var logoPath = Path.Combine(wwwrootPath, "logo.png");
        var promotionPath = Path.Combine(wwwrootPath, "promotion.png");
        
        // Usar Content-ID para incrustar imágenes
        var logoCid = "logo@minimarket";
        var promotionCid = "promotion@minimarket";
        
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f5f5f5;'>
                <div style='max-width: 600px; margin: 0 auto; background-color: white;'>
                    <!-- Logo -->
                    <div style='text-align: center; padding: 30px 20px 20px 20px; background-color: white;'>
                        <img src='cid:{logoCid}' alt='Minimarket Camucha' style='max-width: 200px; height: auto;'>
                    </div>
                    
                    <!-- Título -->
                    <div style='text-align: center; padding: 20px; background-color: white;'>
                        <h1 style='margin: 0; font-size: 28px; color: #333; font-weight: bold;'>¡Pedido Confirmado!</h1>
                    </div>
                    
                    <!-- Cuerpo -->
                    <div style='padding: 30px; background-color: white;'>
                        <p style='font-size: 16px; margin-bottom: 20px; color: #333;'>Estimado/a <strong>{customerName}</strong>,</p>
                        
                        <p style='font-size: 16px; margin-bottom: 20px; color: #333;'>
                            Gracias por tu compra. Tu pedido <strong>#{orderNumber}</strong> ha sido confirmado y está siendo procesado.
                        </p>
                        
                        <!-- Detalles del Pedido -->
                        <div style='background-color: #f9fafb; padding: 20px; border-radius: 8px; margin: 20px 0; border: 1px solid #e5e7eb;'>
                            <h3 style='margin-top: 0; margin-bottom: 15px; color: #333; font-size: 18px;'>Detalles del Pedido</h3>
                            <p style='margin: 10px 0; font-size: 15px; color: #666;'><strong style='color: #333;'>Método de Envío:</strong> {shippingMethodText}</p>
                            <p style='margin: 10px 0; font-size: 15px; color: #666;'><strong style='color: #333;'>Total:</strong> S/ {total:F2}</p>
                            <p style='margin: 10px 0; font-size: 15px; color: #666;'><strong style='color: #333;'>Fecha Estimada:</strong> {deliveryText}</p>
                        </div>
                        
                        <p style='font-size: 16px; margin-top: 30px; color: #333;'>
                            Recibirás una notificación por correo electrónico cuando tu pedido sea despachado o esté listo para retiro.
                        </p>
                        
                        <p style='font-size: 16px; margin-top: 20px; color: #333;'>
                            Si tienes alguna pregunta, no dudes en contactarnos.
                        </p>
                    </div>
                    
                    <!-- Imagen de Promoción -->
                    <div style='text-align: center; padding: 20px; background-color: white;'>
                        <img src='cid:{promotionCid}' alt='Promoción Minimarket Camucha' style='max-width: 100%; height: auto; border-radius: 8px;'>
                    </div>
                    
                    <!-- Footer -->
                    <div style='background-color: #f9fafb; padding: 20px; text-align: center; border-top: 1px solid #e5e7eb;'>
                        <p style='margin: 5px 0; font-size: 14px; color: #666; font-weight: bold;'>Minimarket Camucha</p>
                        <p style='margin: 5px 0; font-size: 12px; color: #999;'>Este es un correo automático, por favor no responder.</p>
                    </div>
                </div>
            </body>
            </html>";

        return await SendEmailWithEmbeddedImagesAsync(toEmail, subject, body, logoPath, logoCid, promotionPath, promotionCid);
    }

    public async Task<bool> SendOrderStatusUpdateAsync(string toEmail, string customerName, string orderNumber, string status, string? trackingUrl = null)
    {
        var statusText = status switch
        {
            "preparing" => "Preparando tu pedido",
            "shipped" => "Tu pedido ha sido despachado",
            "delivered" => "Tu pedido ha sido entregado",
            "ready_for_pickup" => "Tu pedido está listo para retiro",
            _ => "Estado actualizado"
        };

        var subject = $"Actualización de Pedido #{orderNumber} - Minimarket Camucha";
        
        // Cargar imágenes desde el sistema de archivos e incrustarlas
        var wwwrootPath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", "email-templates");
        var logoPath = Path.Combine(wwwrootPath, "logo.png");
        var promotionPath = Path.Combine(wwwrootPath, "promotion.png");
        
        // Usar Content-ID para incrustar imágenes
        var logoCid = "logo@minimarket";
        var promotionCid = "promotion@minimarket";
        
        var trackingSection = !string.IsNullOrEmpty(trackingUrl)
            ? $@"
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{trackingUrl}' style='background: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold;'>
                        Rastrear Pedido
                    </a>
                </div>"
            : "";

        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f5f5f5;'>
                <div style='max-width: 600px; margin: 0 auto; background-color: white;'>
                    <!-- Logo -->
                    <div style='text-align: center; padding: 30px 20px 20px 20px; background-color: white;'>
                        <img src='cid:{logoCid}' alt='Minimarket Camucha' style='max-width: 200px; height: auto;'>
                    </div>
                    
                    <!-- Título -->
                    <div style='text-align: center; padding: 20px; background-color: white;'>
                        <h1 style='margin: 0; font-size: 28px; color: #333; font-weight: bold;'>{statusText}</h1>
                    </div>
                    
                    <!-- Cuerpo -->
                    <div style='padding: 30px; background-color: white;'>
                        <p style='font-size: 16px; margin-bottom: 20px; color: #333;'>Estimado/a <strong>{customerName}</strong>,</p>
                        
                        <p style='font-size: 16px; margin-bottom: 20px; color: #333;'>
                            Te informamos que el estado de tu pedido <strong>#{orderNumber}</strong> ha sido actualizado.
                        </p>
                        
                        <!-- Estado Actual -->
                        <div style='background-color: #f9fafb; padding: 20px; border-radius: 8px; margin: 20px 0; border: 1px solid #e5e7eb;'>
                            <h3 style='margin-top: 0; margin-bottom: 15px; color: #333; font-size: 18px;'>Estado Actual</h3>
                            <p style='margin: 10px 0; font-size: 18px; font-weight: bold; color: #333;'>{statusText}</p>
                        </div>
                        
                        {trackingSection}
                        
                        <p style='font-size: 16px; margin-top: 30px; color: #333;'>
                            Te notificaremos cuando haya más actualizaciones sobre tu pedido.
                        </p>
                    </div>
                    
                    <!-- Imagen de Promoción -->
                    <div style='text-align: center; padding: 20px; background-color: white;'>
                        <img src='cid:{promotionCid}' alt='Promoción Minimarket Camucha' style='max-width: 100%; height: auto; border-radius: 8px;'>
                    </div>
                    
                    <!-- Footer -->
                    <div style='background-color: #f9fafb; padding: 20px; text-align: center; border-top: 1px solid #e5e7eb;'>
                        <p style='margin: 5px 0; font-size: 14px; color: #666; font-weight: bold;'>Minimarket Camucha</p>
                        <p style='margin: 5px 0; font-size: 12px; color: #999;'>Este es un correo automático, por favor no responder.</p>
                    </div>
                </div>
            </body>
            </html>";

        return await SendEmailWithEmbeddedImagesAsync(toEmail, subject, body, logoPath, logoCid, promotionPath, promotionCid);
    }

    public async Task<bool> SendWelcomeEmailAsync(string toEmail, string customerName, string firstName, string lastName)
    {
        var subject = "¡Bienvenido a Minimarket Camucha!";
        
        // Cargar imágenes desde el sistema de archivos e incrustarlas
        var wwwrootPath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", "email-templates");
        var logoPath = Path.Combine(wwwrootPath, "logo.png");
        var promotionPath = Path.Combine(wwwrootPath, "promotion.png");
        
        // Usar Content-ID para incrustar imágenes
        var logoCid = "logo@minimarket";
        var promotionCid = "promotion@minimarket";
        
        // Construir nombre completo
        var fullName = string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName) 
            ? customerName 
            : $"{firstName} {lastName}".Trim();
        
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f5f5f5;'>
                <div style='max-width: 600px; margin: 0 auto; background-color: white;'>
                    <!-- Logo -->
                    <div style='text-align: center; padding: 30px 20px 20px 20px; background-color: white;'>
                        <img src='cid:{logoCid}' alt='Minimarket Camucha' style='max-width: 200px; height: auto;'>
                    </div>
                    
                    <!-- Título -->
                    <div style='text-align: center; padding: 20px; background-color: white;'>
                        <h1 style='margin: 0; font-size: 28px; color: #333; font-weight: bold;'>¡Bienvenido a Minimarket Camucha!</h1>
                    </div>
                    
                    <!-- Cuerpo -->
                    <div style='padding: 30px; background-color: white;'>
                        <p style='font-size: 16px; margin-bottom: 20px; color: #333;'>Estimado/a <strong>{customerName}</strong>,</p>
                        
                        <p style='font-size: 16px; margin-bottom: 20px; color: #333;'>
                            ¡Gracias por registrarte en Minimarket Camucha! Estamos emocionados de tenerte como parte de nuestra comunidad.
                        </p>
                        
                        <!-- Detalles de la Cuenta -->
                        <div style='background-color: #f9fafb; padding: 20px; border-radius: 8px; margin: 20px 0; border: 1px solid #e5e7eb;'>
                            <h3 style='margin-top: 0; margin-bottom: 15px; color: #333; font-size: 18px;'>Detalles de tu Cuenta</h3>
                            <p style='margin: 10px 0; font-size: 15px; color: #666;'><strong style='color: #333;'>Nombre:</strong> {fullName}</p>
                            <p style='margin: 10px 0; font-size: 15px; color: #666;'><strong style='color: #333;'>Correo:</strong> {toEmail}</p>
                        </div>
                        
                        <p style='font-size: 16px; margin-top: 30px; color: #333;'>
                            Ahora puedes explorar nuestros productos, realizar pedidos y disfrutar de nuestras ofertas especiales.
                        </p>
                        
                        <p style='font-size: 16px; margin-top: 20px; color: #333;'>
                            Si tienes alguna pregunta, no dudes en contactarnos. ¡Estamos aquí para ayudarte!
                        </p>
                    </div>
                    
                    <!-- Imagen de Promoción -->
                    <div style='text-align: center; padding: 20px; background-color: white;'>
                        <img src='cid:{promotionCid}' alt='Promoción Minimarket Camucha' style='max-width: 100%; height: auto; border-radius: 8px;'>
                    </div>
                    
                    <!-- Footer -->
                    <div style='background-color: #f9fafb; padding: 20px; text-align: center; border-top: 1px solid #e5e7eb;'>
                        <p style='margin: 5px 0; font-size: 14px; color: #666; font-weight: bold;'>Minimarket Camucha</p>
                        <p style='margin: 5px 0; font-size: 12px; color: #999;'>Este es un correo automático, por favor no responder.</p>
                    </div>
                </div>
            </body>
            </html>";

        return await SendEmailWithEmbeddedImagesAsync(toEmail, subject, body, logoPath, logoCid, promotionPath, promotionCid);
    }

    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string customerName, string resetUrl)
    {
        var subject = "Recuperación de Contraseña - Minimarket Camucha";
        
        // Cargar imágenes desde el sistema de archivos e incrustarlas
        var wwwrootPath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", "email-templates");
        var logoPath = Path.Combine(wwwrootPath, "logo.png");
        var promotionPath = Path.Combine(wwwrootPath, "promotion.png");
        
        // Usar Content-ID para incrustar imágenes
        var logoCid = "logo@minimarket";
        var promotionCid = "promotion@minimarket";
        
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f5f5f5;'>
                <div style='max-width: 600px; margin: 0 auto; background-color: white;'>
                    <!-- Logo -->
                    <div style='text-align: center; padding: 30px 20px 20px 20px; background-color: white;'>
                        <img src='cid:{logoCid}' alt='Minimarket Camucha' style='max-width: 200px; height: auto;'>
                    </div>
                    
                    <!-- Título -->
                    <div style='text-align: center; padding: 20px; background-color: white;'>
                        <h1 style='margin: 0; font-size: 28px; color: #333; font-weight: bold;'>Recuperación de Contraseña</h1>
                    </div>
                    
                    <!-- Cuerpo -->
                    <div style='padding: 30px; background-color: white;'>
                        <p style='font-size: 16px; margin-bottom: 20px; color: #333;'>Estimado/a <strong>{customerName}</strong>,</p>
                        
                        <p style='font-size: 16px; margin-bottom: 20px; color: #333;'>
                            Has solicitado recuperar tu contraseña. Haz clic en el siguiente botón para restablecerla:
                        </p>
                        
                        <!-- Botón de Restablecimiento -->
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetUrl}' style='background: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold; font-size: 16px;'>
                                Restablecer Contraseña
                            </a>
                        </div>
                        
                        <p style='font-size: 14px; margin-top: 30px; color: #666;'>
                            Si el botón no funciona, copia y pega el siguiente enlace en tu navegador:
                        </p>
                        <p style='font-size: 12px; word-break: break-all; color: #2563eb; margin: 10px 0;'>
                            {resetUrl}
                        </p>
                        
                        <p style='font-size: 14px; margin-top: 30px; color: #666;'>
                            <strong>Importante:</strong> Este enlace expirará en 15 minutos. Si no solicitaste este cambio, puedes ignorar este correo de forma segura.
                        </p>
                    </div>
                    
                    <!-- Imagen de Promoción -->
                    <div style='text-align: center; padding: 20px; background-color: white;'>
                        <img src='cid:{promotionCid}' alt='Promoción Minimarket Camucha' style='max-width: 100%; height: auto; border-radius: 8px;'>
                    </div>
                    
                    <!-- Footer -->
                    <div style='background-color: #f9fafb; padding: 20px; text-align: center; border-top: 1px solid #e5e7eb;'>
                        <p style='margin: 5px 0; font-size: 14px; color: #666; font-weight: bold;'>Minimarket Camucha</p>
                        <p style='margin: 5px 0; font-size: 12px; color: #999;'>Este es un correo automático, por favor no responder.</p>
                    </div>
                </div>
            </body>
            </html>";

        return await SendEmailWithEmbeddedImagesAsync(toEmail, subject, body, logoPath, logoCid, promotionPath, promotionCid);
    }

    private async Task<bool> SendEmailWithEmbeddedImagesAsync(string to, string subject, string body, string? logoPath, string logoCid, string? promotionPath, string promotionCid)
    {
        try
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var smtpServer = emailSettings["SmtpServer"];
            var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
            var smtpUser = emailSettings["SmtpUser"];
            var smtpPassword = emailSettings["SmtpPassword"];
            var fromEmail = emailSettings["FromEmail"];
            var fromName = emailSettings["FromName"] ?? "Minimarket Camucha";

            // Si no hay configuración SMTP, usar API REST externa (Resend)
            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUser))
            {
                return await SendEmailViaApiAsync(to, subject, body);
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };

            // Incrustar logo si existe
            if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
            {
                var logo = bodyBuilder.LinkedResources.Add(logoPath);
                logo.ContentId = logoCid;
                logo.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
                logo.ContentDisposition.FileName = "logo.png";
            }

            // Incrustar imagen de promoción si existe
            if (!string.IsNullOrEmpty(promotionPath) && File.Exists(promotionPath))
            {
                var promotion = bodyBuilder.LinkedResources.Add(promotionPath);
                promotion.ContentId = promotionCid;
                promotion.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
                promotion.ContentDisposition.FileName = "promotion.png";
            }

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully with embedded images to {Email}", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email with embedded images to {Email}", to);
            return false;
        }
    }

    private async Task<bool> SendEmailViaApiAsync(string to, string subject, string body, string? attachmentPath = null, string? attachmentName = null)
    {
        try
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var apiKey = emailSettings["ApiKey"]; // Para Resend
            var fromEmail = emailSettings["FromEmail"] ?? "noreply@minimarket.com";
            var fromName = emailSettings["FromName"] ?? "Minimarket Camucha";

            // Usar Resend API
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("No Resend API key found. Using mock email service.");
                // En desarrollo, simular envío exitoso
                return true;
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            // Resend API endpoint
            var apiUrl = "https://api.resend.com/emails";

            var requestBody = new
            {
                from = $"{fromName} <{fromEmail}>",
                to = new[] { to },
                subject = subject,
                html = body
            };

            var response = await httpClient.PostAsJsonAsync(apiUrl, requestBody);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent via Resend API to {Email}", to);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to send email via Resend API. Status: {Status}, Error: {Error}", 
                response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email via Resend API to {Email}", to);
            return false;
        }
    }
}

