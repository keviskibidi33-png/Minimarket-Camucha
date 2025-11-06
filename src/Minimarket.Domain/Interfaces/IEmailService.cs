namespace Minimarket.Domain.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body, string? attachmentPath = null, string? attachmentName = null);
    Task<bool> SendSaleReceiptAsync(string toEmail, string customerName, string saleNumber, string pdfPath, string documentType);
    Task<bool> SendOrderConfirmationAsync(string toEmail, string customerName, string orderNumber, decimal total, string shippingMethod, DateTime? estimatedDelivery);
    Task<bool> SendOrderStatusUpdateAsync(string toEmail, string customerName, string orderNumber, string status, string? trackingUrl = null);
}

