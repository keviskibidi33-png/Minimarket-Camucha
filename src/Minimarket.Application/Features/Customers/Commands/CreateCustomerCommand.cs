using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Customers.DTOs;

namespace Minimarket.Application.Features.Customers.Commands;

public class CreateCustomerCommand : IRequest<Result<CustomerDto>>
{
    public CreateCustomerDto Customer { get; set; } = null!;
}

