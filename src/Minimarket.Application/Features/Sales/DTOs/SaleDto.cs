using Minimarket.Domain.Enums;

namespace Minimarket.Application.Features.Sales.DTOs;

public class SaleDto
{
    public Guid Id { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public DateTime SaleDate { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerDocumentType { get; set; }
    public string? CustomerDocumentNumber { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerEmail { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Change { get; set; }
    public SaleStatus Status { get; set; }
    public string? CancellationReason { get; set; }
    public List<SaleDetailDto> SaleDetails { get; set; } = new();
}

