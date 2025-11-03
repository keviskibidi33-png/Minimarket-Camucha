using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Customers.DTOs;

namespace Minimarket.Application.Features.Customers.Commands;

public class UpdateCustomerCommand : IRequest<Result<CustomerDto>>
{
    public UpdateCustomerDto Customer { get; set; } = null!;
}

