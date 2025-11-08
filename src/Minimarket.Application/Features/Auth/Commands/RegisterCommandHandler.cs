using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Auth.Commands;
using Minimarket.Application.Features.Auth.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Minimarket.Application.Features.Auth.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<LoginResponse>>
{
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly ILogger<RegisterCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        UserManager<IdentityUser<Guid>> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IConfiguration configuration,
        IEmailService emailService,
        ILogger<RegisterCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _emailService = emailService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verificar si el usuario ya existe
            var             existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Result<LoginResponse>.Failure("El correo electrónico ya está registrado");
            }

            // Verificar si el DNI ya está en uso (requerido)
            if (string.IsNullOrWhiteSpace(request.Dni))
            {
                return Result<LoginResponse>.Failure("El DNI es requerido");
            }

            var existingProfile = await _unitOfWork.UserProfiles.FirstOrDefaultAsync(
                up => up.Dni == request.Dni, cancellationToken);
            if (existingProfile != null)
            {
                return Result<LoginResponse>.Failure("El DNI ya está registrado");
            }

            // Verificar si el DNI ya se usa como username
            existingUser = await _userManager.FindByNameAsync(request.Dni);
            if (existingUser != null)
            {
                return Result<LoginResponse>.Failure("El DNI ya está en uso");
            }

            // Crear nuevo usuario usando el DNI como username
            var user = new IdentityUser<Guid>
            {
                UserName = request.Dni, // Usar DNI como username
                Email = request.Email,
                EmailConfirmed = false // Se confirmará por correo
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Error al crear usuario: {Errors}", errors);
                return Result<LoginResponse>.Failure(errors);
            }

            // Asegurar que el rol "Cliente" existe, si no, crearlo
            const string clienteRole = "Cliente";
            if (!await _roleManager.RoleExistsAsync(clienteRole))
            {
                try
                {
                    await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = clienteRole });
                    _logger.LogInformation("Rol 'Cliente' creado automáticamente");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear rol Cliente");
                }
            }

            // Asignar rol de Cliente por defecto
            try
            {
                var roleResult = await _userManager.AddToRoleAsync(user, clienteRole);
                if (!roleResult.Succeeded)
                {
                    _logger.LogWarning("Error al asignar rol Cliente: {Errors}", 
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    // No fallar el proceso si el rol no se puede asignar, pero loguear el error
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al asignar rol Cliente al usuario {UserId}", user.Id);
                // Continuar aunque falle la asignación del rol
            }

            // Crear perfil de usuario básico, siempre incompleto hasta que el usuario lo complete explícitamente
            // Esto fuerza a todos los usuarios nuevos a pasar por el flujo de completar perfil
            try
            {
                var userProfile = new UserProfile
                {
                    UserId = user.Id,
                    FirstName = request.FirstName, // Se guardan si se proporcionan, pero ProfileCompleted sigue siendo false
                    LastName = request.LastName,
                    Dni = request.Dni,
                    Phone = request.Phone,
                    ProfileCompleted = false // Siempre false en el registro inicial, debe completarse en un paso posterior
                };

                await _unitOfWork.UserProfiles.AddAsync(userProfile, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear perfil de usuario para {UserId}", user.Id);
                // Si falla la creación del perfil, el usuario será redirigido a completar perfil de todas formas
            }

            // Generar token JWT
            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

            // Enviar correo de bienvenida en segundo plano (fire-and-forget)
            // Esto no bloquea la respuesta al cliente
            _ = Task.Run(async () =>
            {
                try
                {
                    // Construir nombre completo para el correo
                    var customerName = string.IsNullOrWhiteSpace(request.FirstName) && string.IsNullOrWhiteSpace(request.LastName)
                        ? user.Email?.Split('@')[0] ?? "Usuario"
                        : $"{request.FirstName} {request.LastName}".Trim();
                    
                    await _emailService.SendWelcomeEmailAsync(
                        user.Email ?? string.Empty,
                        customerName,
                        request.FirstName ?? string.Empty,
                        request.LastName ?? string.Empty
                    );
                    _logger.LogInformation("Welcome email sent. Email: {Email}, Name: {FirstName} {LastName}", 
                        user.Email, request.FirstName, request.LastName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send welcome email. Email: {Email}", 
                        user.Email);
                    // No fallar el proceso si el correo falla
                }
            }, cancellationToken);

                // Para el registro inicial, el perfil siempre se considera incompleto hasta que el usuario lo complete explícitamente
                var profileCompleted = false;
                
                // Obtener el perfil recién creado para obtener nombre y apellido
                var createdProfile = await _unitOfWork.UserProfiles.FirstOrDefaultAsync(
                    up => up.UserId == user.Id, cancellationToken);

                return Result<LoginResponse>.Success(new LoginResponse
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddMinutes(expirationMinutes),
                    UserId = user.Id.ToString(),
                    FirstName = createdProfile?.FirstName,
                    LastName = createdProfile?.LastName,
                    Email = user.Email,
                    Roles = roles,
                    ProfileCompleted = profileCompleted
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al registrar usuario. Email: {Email}, DNI: {Dni}", 
                request.Email, request.Dni);
            return Result<LoginResponse>.Failure("Ocurrió un error al registrar el usuario. Por favor, intenta nuevamente.");
        }
    }

    private string GenerateJwtToken(IdentityUser<Guid> user, IList<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

