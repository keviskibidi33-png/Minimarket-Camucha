using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Customers.Commands;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Customers.Commands;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteCustomerCommandHandler> _logger;

    public DeleteCustomerCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteCustomerCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting customer {CustomerId}", request.Id);

        var customer = await _unitOfWork.Customers.GetByIdAsync(request.Id, cancellationToken);

        if (customer == null)
        {
            _logger.LogWarning("Customer not found. CustomerId: {CustomerId}", request.Id);
            throw new NotFoundException("Customer", request.Id);
        }

        // Verificar si tiene ventas asociadas
        var hasSales = (await _unitOfWork.Sales.FindAsync(s => s.CustomerId == request.Id, cancellationToken))
            .Any();

        if (hasSales)
        {
            // En lugar de eliminar, desactivar el cliente
            _logger.LogInformation("Customer has sales, deactivating instead of deleting. CustomerId: {CustomerId}", request.Id);
            customer.IsActive = false;
            customer.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Customers.UpdateAsync(customer, cancellationToken);
        }
        else
        {
            _logger.LogInformation("Customer has no sales, deleting. CustomerId: {CustomerId}", request.Id);
            await _unitOfWork.Customers.DeleteAsync(customer, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer deleted/deactivated successfully. CustomerId: {CustomerId}", request.Id);

        return Result<bool>.Success(true);
    }
}

