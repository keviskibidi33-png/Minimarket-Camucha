using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Customers.Commands;
using Minimarket.Application.Features.Customers.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Customers.Commands;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<CustomerDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCustomerCommandHandler> _logger;

    public UpdateCustomerCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateCustomerCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CustomerDto>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating customer {CustomerId}", request.Customer.Id);

        var customer = await _unitOfWork.Customers.GetByIdAsync(request.Customer.Id, cancellationToken);

        if (customer == null)
        {
            _logger.LogWarning("Customer not found. CustomerId: {CustomerId}", request.Customer.Id);
            throw new NotFoundException("Customer", request.Customer.Id);
        }

        // Verificar si el documento ya existe en otro cliente
        var existingCustomer = (await _unitOfWork.Customers.FindAsync(
            c => c.DocumentType == request.Customer.DocumentType &&
                 c.DocumentNumber == request.Customer.DocumentNumber &&
                 c.Id != request.Customer.Id,
            cancellationToken)).FirstOrDefault();

        if (existingCustomer != null)
        {
            _logger.LogWarning("Attempted to update customer with duplicate document {DocumentType} - {DocumentNumber}. Existing CustomerId: {ExistingCustomerId}", 
                request.Customer.DocumentType, request.Customer.DocumentNumber, existingCustomer.Id);
            throw new BusinessRuleViolationException("Ya existe otro cliente con este documento");
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

        _logger.LogInformation("Customer updated successfully. CustomerId: {CustomerId}", customer.Id);

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

