using MediatR;
using Microsoft.AspNetCore.Identity;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Users.Commands;

public class ResetUserPasswordCommandHandler : IRequestHandler<ResetUserPasswordCommand, Result<string>>
{
    private readonly UserManager<IdentityUser<Guid>> _userManager;

    public ResetUserPasswordCommandHandler(UserManager<IdentityUser<Guid>> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<string>> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return Result<string>.Failure("Usuario no encontrado");
            }

            // Generar token de reset
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // Resetear contraseña
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<string>.Failure($"Error al resetear contraseña: {errors}");
            }

            return Result<string>.Success("Contraseña actualizada exitosamente");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Error al resetear contraseña: {ex.Message}");
        }
    }
}

