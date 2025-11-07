using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.PaymentMethods.Queries;

public class GetPaymentMethodSettingsQuery : IRequest<Result<List<PaymentMethodSettingsDto>>>
{
}

public class PaymentMethodSettingsDto
{
    public Guid Id { get; set; }
    public int PaymentMethodId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool RequiresCardDetails { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}

