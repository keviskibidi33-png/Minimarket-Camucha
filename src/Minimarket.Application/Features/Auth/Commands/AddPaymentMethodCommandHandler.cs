using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Enums;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Auth.Commands;

public class AddPaymentMethodCommandHandler : IRequestHandler<AddPaymentMethodCommand, Result<PaymentMethodResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddPaymentMethodCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaymentMethodResponse>> Handle(AddPaymentMethodCommand request, CancellationToken cancellationToken)
    {
        // Validar que el método de pago "Tarjeta" esté habilitado
        var cardPaymentSetting = await _unitOfWork.PaymentMethodSettings
            .FirstOrDefaultAsync(pms => pms.PaymentMethodId == (int)PaymentMethod.Tarjeta, cancellationToken);
        
        if (cardPaymentSetting == null || !cardPaymentSetting.IsEnabled)
        {
            return Result<PaymentMethodResponse>.Failure("El método de pago con tarjeta no está disponible actualmente");
        }
        
        // Verificar si ya hay métodos de pago para este usuario
        var existingPaymentMethods = await _unitOfWork.UserPaymentMethods
            .FindAsync(upm => upm.UserId == request.UserId, cancellationToken);
        
        var isFirstPaymentMethod = !existingPaymentMethods.Any();
        var shouldBeDefault = isFirstPaymentMethod || request.IsDefault;
        
        // Enmascarar el número de tarjeta
        var cardNumber = request.CardNumber.Replace(" ", "").Replace("-", "");
        var last4Digits = cardNumber.Length >= 4 ? cardNumber.Substring(cardNumber.Length - 4) : cardNumber;
        var maskedCardNumber = $"**** **** **** {last4Digits}";
        
        // Detectar tipo de tarjeta
        var cardType = DetectCardType(cardNumber);
        
        // Validar fecha de expiración
        var currentDate = DateTime.UtcNow;
        if (request.ExpiryYear < currentDate.Year || 
            (request.ExpiryYear == currentDate.Year && request.ExpiryMonth < currentDate.Month))
        {
            return Result<PaymentMethodResponse>.Failure("La tarjeta ha expirado");
        }
        
        // Si este será el método por defecto, desmarcar los demás
        if (shouldBeDefault)
        {
            foreach (var existing in existingPaymentMethods)
            {
                existing.IsDefault = false;
                await _unitOfWork.UserPaymentMethods.UpdateAsync(existing, cancellationToken);
            }
        }
        
        var paymentMethod = new UserPaymentMethod
        {
            UserId = request.UserId,
            CardHolderName = request.CardHolderName,
            CardNumberMasked = maskedCardNumber,
            CardType = cardType,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Last4Digits = last4Digits,
            IsDefault = shouldBeDefault
        };
        
        await _unitOfWork.UserPaymentMethods.AddAsync(paymentMethod, cancellationToken);
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
    
    private string DetectCardType(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
            return "Desconocida";
            
        var firstDigit = cardNumber[0];
        var firstTwoDigits = cardNumber.Length >= 2 ? cardNumber.Substring(0, 2) : "";
        
        if (firstDigit == '4')
            return "Visa";
        if (firstTwoDigits == "51" || firstTwoDigits == "52" || firstTwoDigits == "53" || 
            firstTwoDigits == "54" || firstTwoDigits == "55" || (firstTwoDigits.StartsWith("22") && int.Parse(firstTwoDigits) >= 22 && int.Parse(firstTwoDigits) <= 27))
            return "Mastercard";
        if (firstTwoDigits == "34" || firstTwoDigits == "37")
            return "American Express";
        if (firstTwoDigits == "60" || firstTwoDigits == "65" || firstDigit == '6')
            return "Discover";
            
        return "Desconocida";
    }
}

