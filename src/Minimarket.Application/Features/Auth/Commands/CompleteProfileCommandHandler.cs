using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Auth.Commands;

public class CompleteProfileCommandHandler : IRequestHandler<CompleteProfileCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CompleteProfileCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(CompleteProfileCommand request, CancellationToken cancellationToken)
    {
        // Verificar si el usuario ya tiene un perfil
        var existingProfile = await _unitOfWork.UserProfiles.FirstOrDefaultAsync(
            up => up.UserId == request.UserId, cancellationToken);

        // Verificar si el DNI ya está en uso por otro usuario
        var dniInUse = await _unitOfWork.UserProfiles.FirstOrDefaultAsync(
            up => up.Dni == request.Dni && up.UserId != request.UserId, cancellationToken);

        if (dniInUse != null)
        {
            return Result<string>.Failure("El DNI ya está registrado por otro usuario");
        }

        if (existingProfile != null)
        {
            // Actualizar perfil existente
            existingProfile.FirstName = request.FirstName;
            existingProfile.LastName = request.LastName;
            existingProfile.Dni = request.Dni;
            existingProfile.Phone = request.Phone;
            existingProfile.ProfileCompleted = true;

            await _unitOfWork.UserProfiles.UpdateAsync(existingProfile, cancellationToken);
        }
        else
        {
            // Crear nuevo perfil
            var userProfile = new UserProfile
            {
                UserId = request.UserId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Dni = request.Dni,
                Phone = request.Phone,
                ProfileCompleted = true
            };

            await _unitOfWork.UserProfiles.AddAsync(userProfile, cancellationToken);
        }

        // Si se proporciona un método de pago, guardarlo
        if (request.PaymentMethod != null)
        {
            // Verificar si ya hay métodos de pago para este usuario
            var existingPaymentMethods = await _unitOfWork.UserPaymentMethods
                .FindAsync(upm => upm.UserId == request.UserId, cancellationToken);
            
            var isFirstPaymentMethod = !existingPaymentMethods.Any();
            
            // Enmascarar el número de tarjeta (mostrar solo los últimos 4 dígitos)
            var cardNumber = request.PaymentMethod.CardNumber.Replace(" ", "").Replace("-", "");
            var last4Digits = cardNumber.Length >= 4 ? cardNumber.Substring(cardNumber.Length - 4) : cardNumber;
            var maskedCardNumber = $"**** **** **** {last4Digits}";
            
            // Detectar tipo de tarjeta basado en el primer dígito
            var cardType = DetectCardType(cardNumber);
            
            var paymentMethod = new UserPaymentMethod
            {
                UserId = request.UserId,
                CardHolderName = request.PaymentMethod.CardHolderName,
                CardNumberMasked = maskedCardNumber,
                CardType = cardType,
                ExpiryMonth = request.PaymentMethod.ExpiryMonth,
                ExpiryYear = request.PaymentMethod.ExpiryYear,
                Last4Digits = last4Digits,
                IsDefault = isFirstPaymentMethod || request.PaymentMethod.IsDefault
            };
            
            // Si este es el método por defecto, desmarcar los demás
            if (paymentMethod.IsDefault)
            {
                foreach (var existing in existingPaymentMethods)
                {
                    existing.IsDefault = false;
                    await _unitOfWork.UserPaymentMethods.UpdateAsync(existing, cancellationToken);
                }
            }
            
            await _unitOfWork.UserPaymentMethods.AddAsync(paymentMethod, cancellationToken);
        }

        // Si se proporciona una dirección, guardarla
        if (request.Address != null)
        {
            // Verificar si ya hay direcciones para este usuario
            var existingAddresses = await _unitOfWork.UserAddresses
                .FindAsync(ua => ua.UserId == request.UserId, cancellationToken);
            
            var isFirstAddress = !existingAddresses.Any();
            
            var address = new UserAddress
            {
                UserId = request.UserId,
                Label = request.Address.Label,
                FullName = request.Address.FullName,
                Phone = request.Address.Phone,
                Address = request.Address.Address,
                Reference = request.Address.Reference,
                District = request.Address.District,
                City = request.Address.City,
                Region = request.Address.Region,
                PostalCode = request.Address.PostalCode,
                Latitude = request.Address.Latitude,
                Longitude = request.Address.Longitude,
                IsDefault = isFirstAddress || request.Address.IsDefault
            };
            
            // Si este es la dirección por defecto, desmarcar las demás
            if (address.IsDefault)
            {
                foreach (var existing in existingAddresses)
                {
                    existing.IsDefault = false;
                    await _unitOfWork.UserAddresses.UpdateAsync(existing, cancellationToken);
                }
            }
            
            await _unitOfWork.UserAddresses.AddAsync(address, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("Perfil completado exitosamente");
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

