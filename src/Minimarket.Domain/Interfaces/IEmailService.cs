namespace Minimarket.Domain.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body, string? attachmentPath = null, string? attachmentName = null);
    Task<bool> SendSaleReceiptAsync(string toEmail, string customerName, string saleNumber, string pdfPath, string documentType);
}

