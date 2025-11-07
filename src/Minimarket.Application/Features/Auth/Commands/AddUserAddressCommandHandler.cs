using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Auth.Commands;

public class AddUserAddressCommandHandler : IRequestHandler<AddUserAddressCommand, Result<UserAddressResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddUserAddressCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserAddressResponse>> Handle(AddUserAddressCommand request, CancellationToken cancellationToken)
    {
        // Verificar si ya hay direcciones para este usuario
        var existingAddresses = await _unitOfWork.UserAddresses
            .FindAsync(ua => ua.UserId == request.UserId, cancellationToken);
        
        var isFirstAddress = !existingAddresses.Any();
        var shouldBeDefault = isFirstAddress || request.IsDefault;
        
        // Si este será la dirección por defecto, desmarcar las demás
        if (shouldBeDefault)
        {
            foreach (var existing in existingAddresses)
            {
                existing.IsDefault = false;
                await _unitOfWork.UserAddresses.UpdateAsync(existing, cancellationToken);
            }
        }
        
        var address = new UserAddress
        {
            UserId = request.UserId,
            Label = request.Label,
            IsDifferentRecipient = request.IsDifferentRecipient,
            FullName = request.FullName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Dni = request.Dni,
            Phone = request.Phone,
            Address = request.Address,
            Reference = request.Reference,
            District = request.District,
            City = request.City,
            Region = request.Region,
            PostalCode = request.PostalCode,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsDefault = shouldBeDefault
        };
        
        await _unitOfWork.UserAddresses.AddAsync(address, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result<UserAddressResponse>.Success(new UserAddressResponse
        {
            Id = address.Id,
            Label = address.Label,
            IsDifferentRecipient = address.IsDifferentRecipient,
            FullName = address.FullName,
            FirstName = address.FirstName,
            LastName = address.LastName,
            Dni = address.Dni,
            Phone = address.Phone,
            Address = address.Address,
            Reference = address.Reference,
            District = address.District,
            City = address.City,
            Region = address.Region,
            PostalCode = address.PostalCode,
            Latitude = address.Latitude,
            Longitude = address.Longitude,
            IsDefault = address.IsDefault
        });
    }
}

