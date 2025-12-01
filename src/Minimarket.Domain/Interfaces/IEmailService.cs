namespace Minimarket.Domain.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body, string? attachmentPath = null, string? attachmentName = null);
    Task<bool> SendSaleReceiptAsync(string toEmail, string customerName, string saleNumber, string pdfPath, string documentType, decimal total, DateTime saleDate);
    Task<bool> SendOrderConfirmationAsync(string toEmail, string customerName, string orderNumber, decimal total, string shippingMethod, DateTime? estimatedDelivery);
    Task<bool> SendOrderStatusUpdateAsync(string toEmail, string customerName, string orderNumber, string status, string? trackingUrl = null);
    Task<bool> SendOrderApprovalAsync(string toEmail, string customerName, string orderNumber, decimal total, string paymentMethod, string? pdfPath = null, string? pdfFileName = null);
    Task<bool> SendOrderRejectionAsync(string toEmail, string customerName, string orderNumber, string reason, string? pdfPath = null, string? pdfFileName = null);
    Task<bool> SendPaymentVerifiedAsync(string toEmail, string customerName, string orderNumber, decimal total, string? pdfPath = null, string? pdfFileName = null);
    Task<bool> SendWelcomeEmailAsync(string toEmail, string customerName, string firstName, string lastName);
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string customerName, string resetUrl);
}

