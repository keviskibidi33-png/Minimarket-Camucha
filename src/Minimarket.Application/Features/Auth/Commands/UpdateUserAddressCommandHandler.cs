using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Auth.Commands;

public class UpdateUserAddressCommandHandler : IRequestHandler<UpdateUserAddressCommand, Result<UserAddressResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserAddressCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserAddressResponse>> Handle(UpdateUserAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await _unitOfWork.UserAddresses
            .FirstOrDefaultAsync(ua => ua.Id == request.AddressId && ua.UserId == request.UserId, cancellationToken);
        
        if (address == null)
        {
            return Result<UserAddressResponse>.Failure("Dirección no encontrada");
        }
        
        // Si este será la dirección por defecto, desmarcar las demás
        if (request.IsDefault && !address.IsDefault)
        {
            var existingAddresses = await _unitOfWork.UserAddresses
                .FindAsync(ua => ua.UserId == request.UserId && ua.Id != request.AddressId, cancellationToken);
            
            foreach (var existing in existingAddresses)
            {
                existing.IsDefault = false;
                await _unitOfWork.UserAddresses.UpdateAsync(existing, cancellationToken);
            }
        }
        
        address.Label = request.Label;
        address.IsDifferentRecipient = request.IsDifferentRecipient;
        address.FullName = request.FullName;
        address.FirstName = request.FirstName;
        address.LastName = request.LastName;
        address.Dni = request.Dni;
        address.Phone = request.Phone;
        address.Address = request.Address;
        address.Reference = request.Reference;
        address.District = request.District;
        address.City = request.City;
        address.Region = request.Region;
        address.PostalCode = request.PostalCode;
        address.Latitude = request.Latitude;
        address.Longitude = request.Longitude;
        address.IsDefault = request.IsDefault;
        
        await _unitOfWork.UserAddresses.UpdateAsync(address, cancellationToken);
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

