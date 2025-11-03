using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Customers.Commands;
using Minimarket.Application.Features.Customers.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Customers.Commands;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<CustomerDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CustomerDto>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.Customer.Id, cancellationToken);

        if (customer == null)
        {
            return Result<CustomerDto>.Failure("Cliente no encontrado");
        }

        // Verificar si el documento ya existe en otro cliente
        var existingCustomer = (await _unitOfWork.Customers.FindAsync(
            c => c.DocumentType == request.Customer.DocumentType &&
                 c.DocumentNumber == request.Customer.DocumentNumber &&
                 c.Id != request.Customer.Id,
            cancellationToken)).FirstOrDefault();

        if (existingCustomer != null)
        {
            return Result<CustomerDto>.Failure("Ya existe otro cliente con este documento");
        }

        customer.DocumentType = request.Customer.DocumentType;
        customer.DocumentNumber = request.Customer.DocumentNumber;
        customer.Name = request.Customer.Name;
        customer.Email = request.Customer.Email;
        customer.Phone = request.Customer.Phone;
        customer.Address = request.Customer.Address;
        customer.IsActive = request.Customer.IsActive;
        customer.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Customers.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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

