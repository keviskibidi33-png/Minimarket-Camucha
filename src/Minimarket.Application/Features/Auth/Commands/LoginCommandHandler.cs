using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Auth.Commands;
using Minimarket.Application.Features.Auth.DTOs;
using Minimarket.Application.Features.Auth.Queries;
using Minimarket.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Minimarket.Application.Features.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly SignInManager<IdentityUser<Guid>> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        UserManager<IdentityUser<Guid>> userManager,
        SignInManager<IdentityUser<Guid>> signInManager,
        IConfiguration configuration,
        IUnitOfWork unitOfWork,
        ILogger<LoginCommandHandler> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Intentar buscar por email primero, luego por username
            var user = await _userManager.FindByEmailAsync(request.Username);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(request.Username);
            }
            
            if (user == null)
            {
                // Verificar si es un email válido para dar un mensaje más específico
                var isEmail = request.Username.Contains("@");
                var errorMessage = isEmail 
                    ? "No existe una cuenta asociada a este correo electrónico" 
                    : "No existe una cuenta con este usuario o correo";
                _logger.LogWarning("Intento de login con usuario inexistente: {Username}", request.Username);
                return Result<LoginResponse>.Failure(errorMessage);
            }

            // Verificar si el usuario está bloqueado
            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Intento de login con usuario bloqueado: {UserId}", user.Id);
                return Result<LoginResponse>.Failure("Su cuenta ha sido bloqueada temporalmente. Por favor, intente más tarde.");
            }

            // Verificar contraseña
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            
            if (result.IsLockedOut)
            {
                _logger.LogWarning("Usuario bloqueado por múltiples intentos fallidos: {UserId}", user.Id);
                return Result<LoginResponse>.Failure("Su cuenta ha sido bloqueada temporalmente debido a múltiples intentos fallidos. Por favor, intente más tarde.");
            }

            if (result.IsNotAllowed)
            {
                _logger.LogWarning("Login no permitido para usuario: {UserId}, EmailConfirmed: {EmailConfirmed}", 
                    user.Id, user.EmailConfirmed);
                return Result<LoginResponse>.Failure("No se le permite iniciar sesión. Por favor, verifique su correo electrónico.");
            }

            if (!result.Succeeded)
            {
                _logger.LogWarning("Contraseña incorrecta para usuario: {UserId}", user.Id);
                return Result<LoginResponse>.Failure("La contraseña es incorrecta");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

            // Verificar si el perfil está completo y obtener nombre y apellido
            var profile = await _unitOfWork.UserProfiles.FirstOrDefaultAsync(
                up => up.UserId == user.Id, cancellationToken);
            var profileCompleted = profile?.ProfileCompleted ?? false;

            _logger.LogInformation("Login exitoso para usuario: {UserId}, Email: {Email}, Roles: {Roles}", 
                user.Id, user.Email, string.Join(", ", roles));

            return Result<LoginResponse>.Success(new LoginResponse
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(expirationMinutes),
                UserId = user.Id.ToString(),
                FirstName = profile?.FirstName,
                LastName = profile?.LastName,
                Email = user.Email,
                Roles = roles,
                ProfileCompleted = profileCompleted
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al procesar login para usuario: {Username}", request.Username);
            return Result<LoginResponse>.Failure("Ocurrió un error al procesar el inicio de sesión. Por favor, intente nuevamente.");
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

