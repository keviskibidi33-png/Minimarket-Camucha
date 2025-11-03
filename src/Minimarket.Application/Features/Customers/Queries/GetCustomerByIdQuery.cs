using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Customers.DTOs;

namespace Minimarket.Application.Features.Customers.Queries;

public class GetCustomerByIdQuery : IRequest<Result<CustomerDto>>
{
    public Guid Id { get; set; }
}

