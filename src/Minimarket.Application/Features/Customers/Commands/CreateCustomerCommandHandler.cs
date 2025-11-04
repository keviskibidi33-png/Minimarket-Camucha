using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Customers.Commands;
using Minimarket.Application.Features.Customers.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Customers.Commands;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateCustomerCommandHandler> _logger;

    public CreateCustomerCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateCustomerCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating customer with document {DocumentType} - {DocumentNumber}", 
            request.Customer.DocumentType, request.Customer.DocumentNumber);

        // Verificar si el cliente ya existe
        var existingCustomer = (await _unitOfWork.Customers.FindAsync(
            c => c.DocumentType == request.Customer.DocumentType && 
                 c.DocumentNumber == request.Customer.DocumentNumber,
            cancellationToken)).FirstOrDefault();

        if (existingCustomer != null)
        {
            _logger.LogWarning("Attempted to create duplicate customer with document {DocumentType} - {DocumentNumber}", 
                request.Customer.DocumentType, request.Customer.DocumentNumber);
            throw new BusinessRuleViolationException("Ya existe un cliente con este documento");
        }

        var customer = new Customer
        {
            DocumentType = request.Customer.DocumentType,
            DocumentNumber = request.Customer.DocumentNumber,
            Name = request.Customer.Name,
            Email = request.Customer.Email,
            Phone = request.Customer.Phone,
            Address = request.Customer.Address,
            IsActive = true
        };

        await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer created successfully. CustomerId: {CustomerId}, DocumentNumber: {DocumentNumber}", 
            customer.Id, customer.DocumentNumber);

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

