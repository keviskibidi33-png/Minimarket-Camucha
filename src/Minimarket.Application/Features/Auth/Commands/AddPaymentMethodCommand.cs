using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Auth.Commands;

public class AddPaymentMethodCommand : IRequest<Result<PaymentMethodResponse>>
{
    public Guid UserId { get; set; }
    public string CardHolderName { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public bool IsDefault { get; set; } = false;
}

public class PaymentMethodResponse
{
    public Guid Id { get; set; }
    public string CardHolderName { get; set; } = string.Empty;
    public string CardNumberMasked { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public bool IsDefault { get; set; }
    public string? Last4Digits { get; set; }
}

