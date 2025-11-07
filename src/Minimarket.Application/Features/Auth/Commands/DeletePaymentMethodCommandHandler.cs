using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Auth.Commands;

public class DeletePaymentMethodCommandHandler : IRequestHandler<DeletePaymentMethodCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeletePaymentMethodCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(DeletePaymentMethodCommand request, CancellationToken cancellationToken)
    {
        // Buscar el método de pago y verificar que pertenece al usuario
        var paymentMethod = await _unitOfWork.UserPaymentMethods.FirstOrDefaultAsync(
            upm => upm.Id == request.PaymentMethodId && upm.UserId == request.UserId, 
            cancellationToken);
        
        if (paymentMethod == null)
        {
            return Result<string>.Failure("Método de pago no encontrado");
        }
        
        var wasDefault = paymentMethod.IsDefault;
        
        // Eliminar el método de pago
        await _unitOfWork.UserPaymentMethods.DeleteAsync(paymentMethod, cancellationToken);
        
        // Si era el método por defecto y hay otros métodos, marcar el primero como predeterminado
        if (wasDefault)
        {
            var remainingMethods = await _unitOfWork.UserPaymentMethods
                .FindAsync(upm => upm.UserId == request.UserId, cancellationToken);
            
            var firstMethod = remainingMethods.FirstOrDefault();
            if (firstMethod != null)
            {
                firstMethod.IsDefault = true;
                await _unitOfWork.UserPaymentMethods.UpdateAsync(firstMethod, cancellationToken);
            }
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result<string>.Success("Método de pago eliminado exitosamente");
    }
}

