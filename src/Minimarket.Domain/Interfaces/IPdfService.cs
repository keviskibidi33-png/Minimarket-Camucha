namespace Minimarket.Domain.Interfaces;

public interface IPdfService
{
    Task<string> GenerateSaleReceiptAsync(Guid saleId, string documentType);
    Task<string> GenerateWebOrderReceiptAsync(Guid orderId, string documentType = "Boleta");
    Task<string> GeneratePreviewPdfAsync(string documentType, Dictionary<string, string>? customSettings = null);
    Task<string> GenerateCashClosurePdfAsync(DateTime startDate, DateTime endDate, List<Domain.Entities.Sale> sales);
}

