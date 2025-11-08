using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Auth.Commands;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Auth.Commands;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<string>>
{
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;

    public ForgotPasswordCommandHandler(
        UserManager<IdentityUser<Guid>> userManager,
        IEmailService emailService,
        ILogger<ForgotPasswordCommandHandler> logger,
        IConfiguration configuration,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        
        // Por seguridad, siempre devolver éxito aunque el usuario no exista
        // Esto previene la enumeración de usuarios
        if (user == null)
        {
            _logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
            return Result<string>.Success("Si el correo existe, se enviarán las instrucciones para recuperar tu contraseña");
        }

        // Generar token de recuperación
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        // Obtener la URL del frontend desde la configuración
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:4200";
        
        // Crear URL de recuperación (debe apuntar al frontend, no al backend)
        var resetUrl = $"{frontendUrl}/auth/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(request.Email)}";
        
        // Obtener el nombre del usuario (nombre y apellido o email)
        var customerName = user.Email ?? "Usuario";
        
        // Intentar obtener nombre y apellido del perfil
        var profile = await _unitOfWork.UserProfiles.FirstOrDefaultAsync(
            up => up.UserId == user.Id, cancellationToken);
        if (profile != null && !string.IsNullOrWhiteSpace(profile.FirstName))
        {
            customerName = $"{profile.FirstName} {profile.LastName}".Trim();
        }
        
        try
        {
            await _emailService.SendPasswordResetEmailAsync(
                request.Email,
                customerName,
                resetUrl
            );
            
            _logger.LogInformation("Password reset email sent to {Email}", request.Email);
            return Result<string>.Success("Se ha enviado un correo con las instrucciones para recuperar tu contraseña");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email to {Email}", request.Email);
            return Result<string>.Failure("Error al enviar el correo de recuperación. Por favor intenta más tarde.");
        }
    }
}

