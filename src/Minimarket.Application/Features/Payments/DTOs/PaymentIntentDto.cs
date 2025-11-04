namespace Minimarket.Application.Features.Payments.DTOs;

public class CreatePaymentIntentDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "pen"; // PEN para soles peruanos
    public string? Description { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

public class PaymentIntentResponseDto
{
    public string ClientSecret { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
}

public class ConfirmPaymentDto
{
    public string PaymentIntentId { get; set; } = string.Empty;
    public Guid SaleId { get; set; }
}

