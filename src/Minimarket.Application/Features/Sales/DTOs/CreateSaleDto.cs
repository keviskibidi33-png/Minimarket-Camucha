using Minimarket.Domain.Enums;

namespace Minimarket.Application.Features.Sales.DTOs;

public class CreateSaleDto
{
    public DocumentType DocumentType { get; set; }
    public Guid? CustomerId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Discount { get; set; } = 0;
    public List<CreateSaleDetailDto> SaleDetails { get; set; } = new();
}

