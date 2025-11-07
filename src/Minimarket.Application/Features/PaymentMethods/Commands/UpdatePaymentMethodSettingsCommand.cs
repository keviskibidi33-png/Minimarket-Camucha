using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.PaymentMethods.Queries;

namespace Minimarket.Application.Features.PaymentMethods.Commands;

public class UpdatePaymentMethodSettingsCommand : IRequest<Result<PaymentMethodSettingsDto>>
{
    public Guid Id { get; set; }
    public bool IsEnabled { get; set; }
    public int DisplayOrder { get; set; }
    public string? Description { get; set; }
}

