using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Shipping.Commands;

public class DeleteShippingRateCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
}

