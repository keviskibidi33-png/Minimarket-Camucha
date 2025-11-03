using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Customers.Queries;
using Minimarket.Application.Features.Customers.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Customers.Queries;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCustomerByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.Id, cancellationToken);

        if (customer == null)
        {
            return Result<CustomerDto>.Failure("Cliente no encontrado");
        }

        var result = new CustomerDto
        {
            Id = customer.Id,
            DocumentType = customer.DocumentType,
            DocumentNumber = customer.DocumentNumber,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt
        };

        return Result<CustomerDto>.Success(result);
    }
}

