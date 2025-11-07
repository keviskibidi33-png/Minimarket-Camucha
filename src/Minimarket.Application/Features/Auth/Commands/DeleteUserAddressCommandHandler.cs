using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Auth.Commands;

public class DeleteUserAddressCommandHandler : IRequestHandler<DeleteUserAddressCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserAddressCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(DeleteUserAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await _unitOfWork.UserAddresses
            .FirstOrDefaultAsync(ua => ua.Id == request.AddressId && ua.UserId == request.UserId, cancellationToken);
        
        if (address == null)
        {
            return Result<string>.Failure("Dirección no encontrada");
        }
        
        var wasDefault = address.IsDefault;
        
        await _unitOfWork.UserAddresses.DeleteAsync(address, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // Si la dirección eliminada era la por defecto, asignar la primera disponible como default
        if (wasDefault)
        {
            var remainingAddresses = await _unitOfWork.UserAddresses
                .FindAsync(ua => ua.UserId == request.UserId, cancellationToken);
            
            var firstAddress = remainingAddresses.FirstOrDefault();
            if (firstAddress != null)
            {
                firstAddress.IsDefault = true;
                await _unitOfWork.UserAddresses.UpdateAsync(firstAddress, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
        
        return Result<string>.Success("Dirección eliminada correctamente");
    }
}

