using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Customers.Commands;

public class DeleteCustomerCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
}

