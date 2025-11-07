using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Auth.Commands;

namespace Minimarket.Application.Features.Auth.Commands;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<string>>
{
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        UserManager<IdentityUser<Guid>> userManager,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Buscar usuario por email
        var user = await _userManager.FindByEmailAsync(request.Email);
        
        if (user == null)
        {
            _logger.LogWarning("Password reset attempted for non-existent email: {Email}", request.Email);
            return Result<string>.Failure("El enlace de recuperación no es válido o ha expirado");
        }

        // Restablecer contraseña usando el token
        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Password reset failed for user {Email}. Errors: {Errors}", request.Email, errors);
            
            // Verificar si el error es por token inválido o expirado
            if (result.Errors.Any(e => e.Code == "InvalidToken"))
            {
                return Result<string>.Failure("El enlace de recuperación ha expirado. Por favor, solicita un nuevo enlace.");
            }
            
            return Result<string>.Failure(errors);
        }

        _logger.LogInformation("Password reset successful for user {Email}", request.Email);
        return Result<string>.Success("Tu contraseña ha sido restablecida exitosamente");
    }
}

