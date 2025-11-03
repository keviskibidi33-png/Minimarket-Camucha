namespace Minimarket.Domain.Interfaces;

public interface IPdfService
{
    Task<string> GenerateSaleReceiptAsync(Guid saleId, string documentType);
}

