using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Customers.Commands;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Customers.Commands;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCustomerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.Id, cancellationToken);

        if (customer == null)
        {
            return Result<bool>.Failure("Cliente no encontrado");
        }

        // Verificar si tiene ventas asociadas
        var hasSales = (await _unitOfWork.Sales.FindAsync(s => s.CustomerId == request.Id, cancellationToken))
            .Any();

        if (hasSales)
        {
            // En lugar de eliminar, desactivar el cliente
            customer.IsActive = false;
            customer.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Customers.UpdateAsync(customer, cancellationToken);
        }
        else
        {
            await _unitOfWork.Customers.DeleteAsync(customer, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}

