using MediatR;
using Microsoft.AspNetCore.Identity;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Users.Commands;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result<string>>
{
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(
        UserManager<IdentityUser<Guid>> userManager,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return Result<string>.Failure("Usuario no encontrado");
            }

            // Verificar si es el último administrador
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Administrador"))
            {
                var allAdmins = await _userManager.GetUsersInRoleAsync("Administrador");
                if (allAdmins.Count <= 1)
                {
                    return Result<string>.Failure("No se puede eliminar el último administrador del sistema");
                }
            }

            // Eliminar perfil de usuario
            var profile = await _unitOfWork.UserProfiles.FirstOrDefaultAsync(
                up => up.UserId == request.UserId, cancellationToken);
            
            if (profile != null)
            {
                await _unitOfWork.UserProfiles.DeleteAsync(profile, cancellationToken);
            }

            // Eliminar direcciones del usuario
            var addresses = await _unitOfWork.UserAddresses.FindAsync(
                ua => ua.UserId == request.UserId, cancellationToken);
            
            foreach (var address in addresses)
            {
                await _unitOfWork.UserAddresses.DeleteAsync(address, cancellationToken);
            }

            // Eliminar métodos de pago del usuario
            var paymentMethods = await _unitOfWork.UserPaymentMethods.FindAsync(
                upm => upm.UserId == request.UserId, cancellationToken);
            
            foreach (var paymentMethod in paymentMethods)
            {
                await _unitOfWork.UserPaymentMethods.DeleteAsync(paymentMethod, cancellationToken);
            }

            // Eliminar usuario de Identity
            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                var errors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));
                return Result<string>.Failure($"Error al eliminar usuario: {errors}");
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success("Usuario eliminado exitosamente");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Error al eliminar usuario: {ex.Message}");
        }
    }
}

