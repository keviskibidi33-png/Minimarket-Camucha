using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Auth.Commands;

public class UpdatePaymentMethodCommandHandler : IRequestHandler<UpdatePaymentMethodCommand, Result<PaymentMethodResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePaymentMethodCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaymentMethodResponse>> Handle(UpdatePaymentMethodCommand request, CancellationToken cancellationToken)
    {
        // Buscar el método de pago y verificar que pertenece al usuario
        var paymentMethod = await _unitOfWork.UserPaymentMethods.FirstOrDefaultAsync(
            upm => upm.Id == request.PaymentMethodId && upm.UserId == request.UserId, 
            cancellationToken);
        
        if (paymentMethod == null)
        {
            return Result<PaymentMethodResponse>.Failure("Método de pago no encontrado");
        }
        
        // Validar fecha de expiración
        var currentDate = DateTime.UtcNow;
        if (request.ExpiryYear < currentDate.Year || 
            (request.ExpiryYear == currentDate.Year && request.ExpiryMonth < currentDate.Month))
        {
            return Result<PaymentMethodResponse>.Failure("La tarjeta ha expirado");
        }
        
        // Si este será el método por defecto, desmarcar los demás
        if (request.IsDefault)
        {
            var allUserPaymentMethods = await _unitOfWork.UserPaymentMethods
                .FindAsync(upm => upm.UserId == request.UserId && upm.Id != request.PaymentMethodId, 
                    cancellationToken);
            
            foreach (var existing in allUserPaymentMethods)
            {
                existing.IsDefault = false;
                await _unitOfWork.UserPaymentMethods.UpdateAsync(existing, cancellationToken);
            }
        }
        
        // Actualizar el método de pago
        paymentMethod.CardHolderName = request.CardHolderName;
        paymentMethod.ExpiryMonth = request.ExpiryMonth;
        paymentMethod.ExpiryYear = request.ExpiryYear;
        paymentMethod.IsDefault = request.IsDefault;
        
        await _unitOfWork.UserPaymentMethods.UpdateAsync(paymentMethod, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result<PaymentMethodResponse>.Success(new PaymentMethodResponse
        {
            Id = paymentMethod.Id,
            CardHolderName = paymentMethod.CardHolderName,
            CardNumberMasked = paymentMethod.CardNumberMasked,
            CardType = paymentMethod.CardType,
            ExpiryMonth = paymentMethod.ExpiryMonth,
            ExpiryYear = paymentMethod.ExpiryYear,
            IsDefault = paymentMethod.IsDefault,
            Last4Digits = paymentMethod.Last4Digits
        });
    }
}

