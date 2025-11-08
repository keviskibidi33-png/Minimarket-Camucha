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
        // Verificar si el usuario ya tiene un perfil (debe existir desde el registro)
        var existingProfile = await _unitOfWork.UserProfiles.FirstOrDefaultAsync(
            up => up.UserId == request.UserId, cancellationToken);

        if (existingProfile == null)
        {
            return Result<string>.Failure("No se encontró el perfil del usuario. Por favor, completa el registro primero.");
        }

        // Actualizar solo el teléfono (nombre, apellido y DNI ya están desde el registro)
        existingProfile.Phone = request.Phone;
        existingProfile.ProfileCompleted = true;

        await _unitOfWork.UserProfiles.UpdateAsync(existingProfile, cancellationToken);

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
}

