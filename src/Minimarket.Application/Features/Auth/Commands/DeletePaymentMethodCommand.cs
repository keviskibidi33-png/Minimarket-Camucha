using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Auth.Commands;

public class DeletePaymentMethodCommand : IRequest<Result<string>>
{
    public Guid UserId { get; set; }
    public Guid PaymentMethodId { get; set; }
}

