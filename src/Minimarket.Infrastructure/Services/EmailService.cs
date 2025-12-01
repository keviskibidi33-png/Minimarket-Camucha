using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minimarket.Domain.Interfaces;
using MimeKit;
using System.Net.Http.Json;
using Microsoft.Extensions.Hosting;
using System.IO;

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

    public async Task<bool> SendSaleReceiptAsync(string toEmail, string customerName, string saleNumber, string pdfPath, string documentType, decimal total, DateTime saleDate)
    {
        var documentTypeText = documentType == "Factura" ? "Factura" : "Boleta";
        var subject = $"{documentTypeText} {saleNumber} - Minimarket Camucha";
        
        // Cargar imágenes desde el sistema de archivos e incrustarlas
        var wwwrootPath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", "email-templates");
        var logoPath = Path.Combine(wwwrootPath, "logo.png");
        var promotionPath = Path.Combine(wwwrootPath, "promotion.png");
        
        // Usar Content-ID para incrustar imágenes
        var logoCid = "logo@minimarket";
        var promotionCid = "promotion@minimarket";
        
        // Formatear fecha
        var saleDateText = saleDate.ToString("dd 'de' MMMM, yyyy 'a las' HH:mm", 
            new System.Globalization.CultureInfo("es-PE"));
        
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
                        <h1 style='margin: 0; font-size: 28px; color: #333; font-weight: bold;'>¡{documentTypeText} de Venta Enviada!</h1>
                    </div>
                    
                    <!-- Cuerpo -->
                    <div style='padding: 30px; background-color: white;'>
                        <p style='font-size: 16px; margin-bottom: 20px; color: #333;'>Estimado/a <strong>{customerName}</strong>,</p>
                        
                        <p style='font-size: 16px; margin-bottom: 20px; color: #333;'>
                            Gracias por tu compra. Adjunto encontrarás tu {documentTypeText.ToLower()} de venta número <strong>{saleNumber}</strong>.
                        </p>
                        
                        <!-- Detalles de la Venta -->
                        <div style='background-color: #f9fafb; padding: 20px; border-radius: 8px; margin: 20px 0; border: 1px solid #e5e7eb;'>
                            <h3 style='margin-top: 0; margin-bottom: 15px; color: #333; font-size: 18px;'>Detalles de la Venta</h3>
                            <p style='margin: 10px 0; font-size: 15px; color: #666;'><strong style='color: #333;'>Documento:</strong> {documentTypeText} {saleNumber}</p>
                            <p style='margin: 10px 0; font-size: 15px; color: #666;'><strong style='color: #333;'>Total:</strong> S/ {total:F2}</p>
                            <p style='margin: 10px 0; font-size: 15px; color: #666;'><strong style='color: #333;'>Fecha:</strong> {saleDateText}</p>
                        </div>
                        
                        <p style='font-size: 16px; margin-top: 30px; color: #333;'>
                            El documento PDF está adjunto a este correo. Puedes descargarlo y guardarlo para tus registros.
                        </p>
                        
                        <p style='font-size: 16px; margin-top: 20px; color: #333;'>
                            Si tienes alguna pregunta sobre tu compra, no dudes en contactarnos. ¡Estamos aquí para ayudarte!
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

        // Usar SendEmailWithEmbeddedImagesAndAttachmentAsync para combinar imágenes incrustadas + adjunto PDF
        return await SendEmailWithEmbeddedImagesAndAttachmentAsync(toEmail, subject, body, logoPath, logoCid, promotionPath, promotionCid, pdfPath, $"{documentTypeText}_{saleNumber}.pdf");
    }

    private async Task<bool> SendEmailWithEmbeddedImagesAndAttachmentAsync(string to, string subject, string body, string? logoPath, string logoCid, string? promotionPath, string promotionCid, string? attachmentPath, string? attachmentName)
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

            // Log de configuración SMTP (sin mostrar la contraseña completa por seguridad)
            _logger.LogInformation("SMTP Configuration - Server: {Server}, Port: {Port}, User: {User}, PasswordLength: {PasswordLength}, FromEmail: {FromEmail}",
                smtpServer, smtpPort, smtpUser, smtpPassword?.Length ?? 0, fromEmail);

            // Si no hay configuración SMTP, usar API REST externa (Resend)
            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUser))
            {
                _logger.LogWarning("SMTP server or user not configured. Using Resend API as fallback.");
                return await SendEmailViaApiAsync(to, subject, body, attachmentPath, attachmentName);
            }

            // Validar que la contraseña no esté vacía
            if (string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogError("SMTP password is empty. Cannot authenticate.");
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

            // Agregar PDF como adjunto (leer en memoria para evitar bloqueo de archivos)
            if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath))
            {
                try
                {
                    // Leer el archivo en memoria con reintentos para evitar bloqueos
                    byte[] pdfBytes = null;
                    int maxRetries = 5;
                    int retryDelay = 100; // milisegundos
                    
                    for (int i = 0; i < maxRetries; i++)
                    {
                        try
                        {
                            // Intentar leer el archivo con FileShare.ReadWrite para permitir acceso compartido
                            using (var fileStream = new FileStream(attachmentPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                using (var memoryStream = new MemoryStream())
                                {
                                    await fileStream.CopyToAsync(memoryStream);
                                    pdfBytes = memoryStream.ToArray();
                                    break; // Éxito, salir del loop
                                }
                            }
                        }
                        catch (IOException ioEx) when (ioEx.Message.Contains("being used by another process"))
                        {
                            if (i < maxRetries - 1)
                            {
                                _logger.LogWarning("Archivo PDF bloqueado, reintentando en {Delay}ms (intento {Attempt}/{MaxRetries})", retryDelay, i + 1, maxRetries);
                                await Task.Delay(retryDelay);
                                retryDelay *= 2; // Backoff exponencial
                            }
                            else
                            {
                                _logger.LogError(ioEx, "No se pudo leer el archivo PDF después de {MaxRetries} intentos: {FilePath}", maxRetries, attachmentPath);
                                throw;
                            }
                        }
                    }
                    
                    if (pdfBytes != null && pdfBytes.Length > 0)
                    {
                        // Crear un MemoryStream desde los bytes (no usar using, MailKit lo manejará)
                        var pdfStream = new MemoryStream(pdfBytes);
                        var attachment = bodyBuilder.Attachments.Add(attachmentName ?? Path.GetFileName(attachmentPath), pdfStream);
                        _logger.LogInformation("PDF adjunto cargado en memoria: {Size} bytes", pdfBytes.Length);
                    }
                    else
                    {
                        _logger.LogWarning("El archivo PDF está vacío o no se pudo leer: {FilePath}", attachmentPath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al leer el archivo PDF para adjuntar: {FilePath}", attachmentPath);
                    // Continuar sin el adjunto en lugar de fallar completamente
                }
            }

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
            
            try
            {
                await client.AuthenticateAsync(smtpUser, smtpPassword);
            }
            catch (MailKit.Security.AuthenticationException authEx)
            {
                _logger.LogWarning(authEx, "SMTP authentication failed. Attempting to use Resend API as fallback. Error: {ErrorMessage}", authEx.Message);
                await client.DisconnectAsync(true);
                // Intentar usar Resend API como fallback
                return await SendEmailViaApiAsync(to, subject, body, attachmentPath, attachmentName);
            }
            
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully with embedded images and attachment to {Email}", to);
            return true;
        }
        catch (MailKit.Security.AuthenticationException authEx)
        {
            _logger.LogWarning(authEx, "SMTP authentication failed for {Email}. Attempting to use Resend API as fallback. Error: {ErrorMessage}", to, authEx.Message);
            // Intentar usar Resend API como fallback
            return await SendEmailViaApiAsync(to, subject, body, attachmentPath, attachmentName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email with embedded images and attachment to {Email}. Error: {ErrorMessage}", to, ex.Message);
            // Intentar usar Resend API como último recurso
            try
            {
                _logger.LogInformation("Attempting to use Resend API as fallback for {Email}", to);
                return await SendEmailViaApiAsync(to, subject, body, attachmentPath, attachmentName);
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "Resend API fallback also failed for {Email}", to);
                return false;
            }
        }
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

            // Log de configuración SMTP (sin mostrar la contraseña completa por seguridad)
            _logger.LogInformation("SMTP Configuration - Server: {Server}, Port: {Port}, User: {User}, PasswordLength: {PasswordLength}, FromEmail: {FromEmail}",
                smtpServer, smtpPort, smtpUser, smtpPassword?.Length ?? 0, fromEmail);

            // Si no hay configuración SMTP, usar API REST externa (Resend)
            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUser))
            {
                _logger.LogWarning("SMTP server or user not configured. Using Resend API as fallback.");
                return await SendEmailViaApiAsync(to, subject, body);
            }

            // Validar que la contraseña no esté vacía
            if (string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogError("SMTP password is empty. Cannot authenticate.");
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
            
            try
            {
                await client.AuthenticateAsync(smtpUser, smtpPassword);
            }
            catch (MailKit.Security.AuthenticationException authEx)
            {
                _logger.LogWarning(authEx, "SMTP authentication failed. Attempting to use Resend API as fallback. Error: {ErrorMessage}", authEx.Message);
                await client.DisconnectAsync(true);
                // Intentar usar Resend API como fallback
                return await SendEmailViaApiAsync(to, subject, body);
            }
            
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully with embedded images to {Email}", to);
            return true;
        }
        catch (MailKit.Security.AuthenticationException authEx)
        {
            _logger.LogWarning(authEx, "SMTP authentication failed for {Email}. Attempting to use Resend API as fallback. Error: {ErrorMessage}", to, authEx.Message);
            // Intentar usar Resend API como fallback
            return await SendEmailViaApiAsync(to, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email with embedded images to {Email}. Error: {ErrorMessage}", to, ex.Message);
            // Intentar usar Resend API como último recurso
            try
            {
                _logger.LogInformation("Attempting to use Resend API as fallback for {Email}", to);
                return await SendEmailViaApiAsync(to, subject, body);
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "Resend API fallback also failed for {Email}", to);
                return false;
            }
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
                _logger.LogError("No Resend API key found in EmailSettings. Cannot send email via Resend API. Please configure ApiKey in appsettings.json or fix SMTP credentials.");
                return false;
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            // Resend API endpoint
            var apiUrl = "https://api.resend.com/emails";

            // Preparar adjunto si existe
            var attachments = new List<object>();
            if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath))
            {
                var fileBytes = await File.ReadAllBytesAsync(attachmentPath);
                var base64Content = Convert.ToBase64String(fileBytes);
                attachments.Add(new
                {
                    filename = attachmentName ?? Path.GetFileName(attachmentPath),
                    content = base64Content
                });
            }

            var requestBody = new
            {
                from = $"{fromName} <{fromEmail}>",
                to = new[] { to },
                subject = subject,
                html = body,
                attachments = attachments.Any() ? attachments : null
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

    public async Task<bool> SendOrderApprovalAsync(string toEmail, string customerName, string orderNumber, decimal total, string paymentMethod, string? pdfPath = null, string? pdfFileName = null)
    {
        var subject = $"¡Pedido #{orderNumber} Aprobado! - Minimarket Camucha";
        
        var wwwrootPath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", "email-templates");
        var logoPath = Path.Combine(wwwrootPath, "logo.png");
        var promotionPath = Path.Combine(wwwrootPath, "promotion.png");
        
        var logoCid = "logo@minimarket";
        var promotionCid = "promotion@minimarket";

        var paymentMethodText = paymentMethod switch
        {
            "cash" => "Efectivo al recibir",
            "bank" => "Transferencia Bancaria",
            "wallet" => "Yape/Plin",
            _ => paymentMethod
        };

        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f5f5f5;'>
                <div style='max-width: 600px; margin: 0 auto; background-color: white;'>
                    <!-- Logo -->
                    <div style='text-align: center; padding: 30px 20px 20px 20px; background-color: white;'>
                        <img src='cid:{logoCid}' alt='Minimarket Camucha' style='max-width: 200px; height: auto;'>
                    </div>
                    
                    <!-- Título -->
                    <div style='text-align: center; padding: 20px; background: linear-gradient(135deg, #10b981 0%, #059669 100%); color: white;'>
                        <h1 style='margin: 0; font-size: 28px; font-weight: bold;'>¡Pedido Aprobado!</h1>
                    </div>
                    
                    <!-- Cuerpo -->
                    <div style='padding: 30px; background-color: white;'>
                        <p style='font-size: 16px; margin-bottom: 20px; color: #333;'>Estimado/a <strong>{customerName}</strong>,</p>
                        
                        <p style='font-size: 16px; margin-bottom: 20px; color: #333;'>
                            Nos complace informarte que tu pedido <strong>#{orderNumber}</strong> ha sido <strong style='color: #10b981;'>aprobado</strong> y está siendo preparado con cuidado.
                        </p>
                        
                        <!-- Información del Pedido -->
                        <div style='background-color: #f9fafb; padding: 20px; border-radius: 8px; margin: 20px 0; border: 1px solid #e5e7eb;'>
                            <h3 style='margin-top: 0; margin-bottom: 15px; color: #333; font-size: 18px;'>Detalles del Pedido</h3>
                            <p style='margin: 10px 0; color: #666;'><strong>Número de Pedido:</strong> #{orderNumber}</p>
                            <p style='margin: 10px 0; color: #666;'><strong>Método de Pago:</strong> {paymentMethodText}</p>
                            <p style='margin: 10px 0; color: #666;'><strong>Total:</strong> S/ {total:F2}</p>
                        </div>
                        
                        <!-- Nota sobre PDF adjunto -->
                        <div style='background-color: #eff6ff; padding: 15px; border-radius: 8px; margin: 20px 0; border: 1px solid #bfdbfe;'>
                            <p style='margin: 0; font-size: 14px; color: #1e40af;'>
                                <strong>📄 Documento Adjunto:</strong> Se adjunta la boleta de tu pedido en formato PDF para tus registros.
                            </p>
                        </div>
                        
                        <p style='font-size: 16px; margin-top: 30px; color: #333;'>
                            Te notificaremos cuando tu pedido esté listo para ser enviado o retirado.
                        </p>
                        
                        <p style='font-size: 16px; margin-top: 20px; color: #333;'>
                            ¡Gracias por confiar en nosotros!
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

        // Si hay PDF, usar método con adjunto; si no, usar método sin adjunto
        if (!string.IsNullOrEmpty(pdfPath) && System.IO.File.Exists(pdfPath))
        {
            return await SendEmailWithEmbeddedImagesAndAttachmentAsync(toEmail, subject, body, logoPath, logoCid, promotionPath, promotionCid, pdfPath, pdfFileName ?? $"Boleta_{orderNumber}.pdf");
        }
        else
        {
            return await SendEmailWithEmbeddedImagesAsync(toEmail, subject, body, logoPath, logoCid, promotionPath, promotionCid);
        }
    }

    public async Task<bool> SendOrderRejectionAsync(string toEmail, string customerName, string orderNumber, string reason, string? pdfPath = null, string? pdfFileName = null)
    {
        try
        {
            _logger.LogInformation("Iniciando envío de correo de rechazo. OrderNumber: {OrderNumber}, Email: {Email}, PDF: {PdfPath}", 
                orderNumber, toEmail, pdfPath ?? "Sin PDF");
            
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogError("No se puede enviar correo de rechazo: email vacío. OrderNumber: {OrderNumber}", orderNumber);
                return false;
            }

            var subject = $"Pedido #{orderNumber} Rechazado - Minimarket Camucha";
            
            var wwwrootPath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", "email-templates");
            var logoPath = Path.Combine(wwwrootPath, "logo.png");
            var promotionPath = Path.Combine(wwwrootPath, "promotion.png");
            
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
                    <div style='text-align: center; padding: 20px; background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%); color: white;'>
                        <h1 style='margin: 0; font-size: 28px; font-weight: bold;'>Pedido Rechazado</h1>
                    </div>
                    
                    <!-- Cuerpo -->
                    <div style='padding: 30px; background-color: white;'>
                        <p style='font-size: 16px; margin-bottom: 20px; color: #333;'>Estimado/a <strong>{customerName}</strong>,</p>
                        
                        <p style='font-size: 16px; margin-bottom: 20px; color: #333;'>
                            Lamentamos informarte que tu pedido <strong>#{orderNumber}</strong> ha sido <strong style='color: #ef4444;'>rechazado</strong>.
                        </p>
                        
                        <!-- Motivo -->
                        <div style='background-color: #fef2f2; padding: 20px; border-radius: 8px; margin: 20px 0; border: 1px solid #fecaca;'>
                            <h3 style='margin-top: 0; margin-bottom: 15px; color: #991b1b; font-size: 18px;'>Motivo del Rechazo</h3>
                            <p style='margin: 10px 0; color: #7f1d1d;'>{reason}</p>
                        </div>
                        
                        <!-- Nota sobre PDF adjunto -->
                        <div style='background-color: #fef2f2; padding: 15px; border-radius: 8px; margin: 20px 0; border: 1px solid #fecaca;'>
                            <p style='margin: 0; font-size: 14px; color: #991b1b;'>
                                <strong>📄 Documento Adjunto:</strong> Se adjunta la boleta de tu pedido en formato PDF para tus registros.
                            </p>
                        </div>
                        
                        <p style='font-size: 16px; margin-top: 30px; color: #333;'>
                            Si tienes alguna pregunta o deseas realizar un nuevo pedido, no dudes en contactarnos.
                        </p>
                        
                        <p style='font-size: 16px; margin-top: 20px; color: #333;'>
                            Estamos aquí para ayudarte.
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

            // Si hay PDF, usar método con adjunto; si no, usar método sin adjunto
            bool result;
            if (!string.IsNullOrEmpty(pdfPath) && System.IO.File.Exists(pdfPath))
            {
                _logger.LogInformation("Enviando correo de rechazo con PDF adjunto. OrderNumber: {OrderNumber}", orderNumber);
                result = await SendEmailWithEmbeddedImagesAndAttachmentAsync(toEmail, subject, body, logoPath, logoCid, promotionPath, promotionCid, pdfPath, pdfFileName ?? $"Boleta_{orderNumber}.pdf");
            }
            else
            {
                _logger.LogInformation("Enviando correo de rechazo sin PDF adjunto. OrderNumber: {OrderNumber}", orderNumber);
                result = await SendEmailWithEmbeddedImagesAsync(toEmail, subject, body, logoPath, logoCid, promotionPath, promotionCid);
            }

            if (result)
            {
                _logger.LogInformation("Correo de rechazo enviado exitosamente. OrderNumber: {OrderNumber}, Email: {Email}", orderNumber, toEmail);
            }
            else
            {
                _logger.LogWarning("El envío de correo de rechazo retornó false. OrderNumber: {OrderNumber}, Email: {Email}", orderNumber, toEmail);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en SendOrderRejectionAsync. OrderNumber: {OrderNumber}, Email: {Email}", orderNumber, toEmail ?? "N/A");
            return false;
        }
    }

    public async Task<bool> SendPaymentVerifiedAsync(string toEmail, string customerName, string orderNumber, decimal total, string? pdfPath = null, string? pdfFileName = null)
    {
        var subject = $"Pago Verificado - Pedido #{orderNumber} - Minimarket Camucha";
        
        var wwwrootPath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", "email-templates");
        var logoPath = Path.Combine(wwwrootPath, "logo.png");
        var promotionPath = Path.Combine(wwwrootPath, "promotion.png");
        
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
                    <div style='text-align: center; padding: 20px; background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%); color: white;'>
                        <h1 style='margin: 0; font-size: 28px; font-weight: bold;'>Pago Verificado</h1>
                    </div>
                    
                    <!-- Cuerpo -->
                    <div style='padding: 30px; background-color: white;'>
                        <p style='font-size: 16px; margin-bottom: 20px; color: #333;'>Estimado/a <strong>{customerName}</strong>,</p>
                        
                        <p style='font-size: 16px; margin-bottom: 20px; color: #333;'>
                            Confirmamos que hemos recibido y <strong style='color: #3b82f6;'>verificado</strong> tu comprobante de pago para el pedido <strong>#{orderNumber}</strong>.
                        </p>
                        
                        <!-- Información del Pago -->
                        <div style='background-color: #eff6ff; padding: 20px; border-radius: 8px; margin: 20px 0; border: 1px solid #bfdbfe;'>
                            <h3 style='margin-top: 0; margin-bottom: 15px; color: #1e40af; font-size: 18px;'>Detalles del Pago</h3>
                            <p style='margin: 10px 0; color: #1e3a8a;'><strong>Número de Pedido:</strong> #{orderNumber}</p>
                            <p style='margin: 10px 0; color: #1e3a8a;'><strong>Monto Verificado:</strong> S/ {total:F2}</p>
                            <p style='margin: 10px 0; color: #1e3a8a;'><strong>Estado:</strong> <span style='color: #10b981; font-weight: bold;'>✓ Verificado</span></p>
                        </div>
                        
                        <!-- Nota sobre PDF adjunto -->
                        <div style='background-color: #eff6ff; padding: 15px; border-radius: 8px; margin: 20px 0; border: 1px solid #bfdbfe;'>
                            <p style='margin: 0; font-size: 14px; color: #1e40af;'>
                                <strong>📄 Documento Adjunto:</strong> Se adjunta la boleta de tu pedido en formato PDF para tus registros.
                            </p>
                        </div>
                        
                        <p style='font-size: 16px; margin-top: 30px; color: #333;'>
                            Tu pedido está siendo procesado y preparado. Te notificaremos cuando esté listo para ser enviado o retirado.
                        </p>
                        
                        <p style='font-size: 16px; margin-top: 20px; color: #333;'>
                            ¡Gracias por tu compra!
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

        // Si hay PDF, usar método con adjunto; si no, usar método sin adjunto
        if (!string.IsNullOrEmpty(pdfPath) && System.IO.File.Exists(pdfPath))
        {
            return await SendEmailWithEmbeddedImagesAndAttachmentAsync(toEmail, subject, body, logoPath, logoCid, promotionPath, promotionCid, pdfPath, pdfFileName ?? $"Boleta_{orderNumber}.pdf");
        }
        else
        {
            return await SendEmailWithEmbeddedImagesAsync(toEmail, subject, body, logoPath, logoCid, promotionPath, promotionCid);
        }
    }
}

