using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Auth.Commands;

namespace Minimarket.Application.Features.Auth.Queries;

public class GetPaymentMethodsQuery : IRequest<Result<List<PaymentMethodResponse>>>
{
    public Guid UserId { get; set; }
}

