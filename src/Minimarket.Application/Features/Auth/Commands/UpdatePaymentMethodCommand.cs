using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Auth.Commands;

public class UpdatePaymentMethodCommand : IRequest<Result<PaymentMethodResponse>>
{
    public Guid UserId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public string CardHolderName { get; set; } = string.Empty;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public bool IsDefault { get; set; }
}

