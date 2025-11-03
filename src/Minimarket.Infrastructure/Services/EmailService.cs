using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minimarket.Domain.Interfaces;
using MimeKit;

namespace Minimarket.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
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

    private async Task<bool> SendEmailViaApiAsync(string to, string subject, string body, string? attachmentPath = null, string? attachmentName = null)
    {
        try
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var apiKey = emailSettings["ApiKey"]; // Para Resend o SendGrid
            var apiUrl = emailSettings["ApiUrl"]; // URL de la API externa
            var fromEmail = emailSettings["FromEmail"] ?? "noreply@minimarket.com";

            // Usar Resend API como ejemplo (puede cambiarse a SendGrid, Mailgun, etc.)
            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiUrl))
            {
                _logger.LogWarning("No email configuration found. Using mock email service.");
                // En desarrollo, simular envío exitoso
                return true;
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var requestBody = new
            {
                from = fromEmail,
                to = new[] { to },
                subject = subject,
                html = body
            };

            var response = await httpClient.PostAsJsonAsync(apiUrl, requestBody);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent via API to {Email}", to);
                return true;
            }

            _logger.LogError("Failed to send email via API. Status: {Status}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email via API to {Email}", to);
            return false;
        }
    }
}

