using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Features.Auth.Commands;
using Minimarket.Application.Features.Auth.DTOs;
using Minimarket.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly SignInManager<IdentityUser<Guid>> _signInManager;
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private readonly IFileStorageService _fileStorageService;

    public AuthController(
        IMediator mediator,
        SignInManager<IdentityUser<Guid>> signInManager,
        UserManager<IdentityUser<Guid>> userManager,
        IConfiguration configuration,
        ILogger<AuthController> logger,
        IFileStorageService fileStorageService)
    {
        _mediator = mediator;
        _signInManager = signInManager;
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
        _fileStorageService = fileStorageService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new { 
                succeeded = false,
                errors = result.Errors,
                message = result.Error
            });
        }

        return Ok(result.Data);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        // Dejar que el GlobalExceptionHandlerMiddleware maneje todas las excepciones
        // ValidationException será capturada y devuelta como 400 BadRequest
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new { 
                succeeded = false,
                errors = result.Errors,
                message = result.Error
            });
        }

        return Ok(result.Data);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new { 
                succeeded = false,
                errors = result.Errors,
                message = result.Error
            });
        }

        return Ok(new { message = result.Data });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new { 
                succeeded = false,
                errors = result.Errors,
                message = result.Error
            });
        }

        return Ok(new { message = result.Data });
    }

    [HttpGet("google-login")]
    [AllowAnonymous]
    public IActionResult GoogleLogin()
    {
        var redirectUrl = Url.Action("GoogleCallback", "Auth", null, Request.Scheme);
        var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
        return Challenge(properties, "Google");
    }

    [HttpGet("google-callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleCallback()
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return Redirect($"{_configuration["BaseUrl"]}/auth/login?error=google_auth_failed");
        }

        // Buscar si el usuario ya existe
        var user = await _userManager.FindByEmailAsync(info.Principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty);
        
        if (user == null)
        {
            // Crear nuevo usuario
            user = new IdentityUser<Guid>
            {
                UserName = info.Principal.FindFirstValue(ClaimTypes.Email)?.Split('@')[0] ?? Guid.NewGuid().ToString(),
                Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return Redirect($"{_configuration["BaseUrl"]}/auth/login?error=user_creation_failed");
            }

            // Asignar rol de Cliente
            await _userManager.AddToRoleAsync(user, "Cliente");
        }

        // Agregar login externo si no existe
        var addLoginResult = await _userManager.AddLoginAsync(user, info);
        if (!addLoginResult.Succeeded && !addLoginResult.Errors.Any(e => e.Code == "LoginAlreadyAssociated"))
        {
            return Redirect($"{_configuration["BaseUrl"]}/auth/login?error=login_association_failed");
        }

        // Iniciar sesión
        await _signInManager.SignInAsync(user, isPersistent: false);

        // Generar token JWT
        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);

        // Verificar si el perfil está completo
        var profileCompleted = await CheckProfileCompleted(user.Id);

        // Redirigir al frontend con el token
        var frontendUrl = _configuration["BaseUrl"]?.Replace(":5000", ":4200") ?? "http://localhost:4200";
        var redirectUrl = profileCompleted 
            ? $"{frontendUrl}/?token={token}" 
            : $"{frontendUrl}/auth/complete-profile?token={token}";

        return Redirect(redirectUrl);
    }

    private async Task<bool> CheckProfileCompleted(Guid userId)
    {
        // Esta lógica se puede mover a un servicio o query
        // Por ahora, verificamos directamente
        using var scope = HttpContext.RequestServices.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var profile = await unitOfWork.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId);
        return profile?.ProfileCompleted ?? false;
    }

    private string GenerateJwtToken(IdentityUser<Guid> user, IList<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

        var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(secretKey!));
        var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
            key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpPost("google-signin")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleSignIn([FromBody] GoogleSignInRequest request)
    {
        if (string.IsNullOrEmpty(request.Credential))
        {
            return BadRequest(new { message = "El token de Google es requerido" });
        }

        try
        {
            // Validar el token de Google usando HttpClient
            using var httpClient = new HttpClient();
            var validationUrl = $"https://oauth2.googleapis.com/tokeninfo?id_token={request.Credential}";
            var response = await httpClient.GetAsync(validationUrl);

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest(new { message = "Token de Google inválido" });
            }

            var content = await response.Content.ReadAsStringAsync();
            var tokenInfo = System.Text.Json.JsonSerializer.Deserialize<GoogleTokenInfo>(content);

            if (tokenInfo == null || string.IsNullOrEmpty(tokenInfo.Email))
            {
                return BadRequest(new { message = "No se pudo obtener la información del usuario de Google" });
            }

            // Buscar si el usuario ya existe
            var user = await _userManager.FindByEmailAsync(tokenInfo.Email);
            
            if (user == null)
            {
                // Crear nuevo usuario
                user = new IdentityUser<Guid>
                {
                    UserName = tokenInfo.Email.Split('@')[0],
                    Email = tokenInfo.Email,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return BadRequest(new { message = "Error al crear el usuario", errors = createResult.Errors });
                }

                // Asignar rol de Cliente
                await _userManager.AddToRoleAsync(user, "Cliente");
            }

            // Generar token JWT
            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);

            // Verificar si el perfil está completo y obtener nombre y apellido
            var profileCompleted = await CheckProfileCompleted(user.Id);
            
            // Obtener perfil para nombre y apellido
            using var scope = HttpContext.RequestServices.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var profile = await unitOfWork.UserProfiles.FirstOrDefaultAsync(up => up.UserId == user.Id);

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

            return Ok(new LoginResponse
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
            return StatusCode(500, new { message = "Error al procesar el inicio de sesión con Google", error = ex.Message });
        }
    }

    [HttpPost("complete-profile")]
    [Authorize]
    public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileCommand command)
    {
        // Obtener el UserId del token JWT
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        command.UserId = userId;
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new { 
                succeeded = false,
                errors = result.Errors,
                message = result.Error
            });
        }

        return Ok(new { message = result.Data });
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        var query = new Minimarket.Application.Features.Auth.Queries.GetUserProfileQuery
        {
            UserId = userId
        };

        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(new { 
                succeeded = false,
                errors = result.Errors,
                message = result.Error
            });
        }

        return Ok(result.Data);
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        var command = new Minimarket.Application.Features.Auth.Commands.UpdateProfileCommand
        {
            UserId = userId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone
            // DNI no se incluye porque no se puede modificar
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new { 
                succeeded = false,
                errors = result.Errors,
                message = result.Error
            });
        }

        return Ok(new { message = result.Data });
    }


    [HttpGet("payment-methods")]
    [Authorize]
    public async Task<IActionResult> GetPaymentMethods()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        var query = new Minimarket.Application.Features.Auth.Queries.GetPaymentMethodsQuery
        {
            UserId = userId
        };

        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(new { 
                succeeded = false,
                errors = result.Errors,
                message = result.Error
            });
        }

        return Ok(result.Data);
    }

    [HttpPost("payment-methods")]
    [Authorize]
    public async Task<IActionResult> AddPaymentMethod([FromBody] AddPaymentMethodRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        var command = new Minimarket.Application.Features.Auth.Commands.AddPaymentMethodCommand
        {
            UserId = userId,
            CardHolderName = request.CardHolderName,
            CardNumber = request.CardNumber,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            IsDefault = request.IsDefault
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new { 
                succeeded = false,
                errors = result.Errors,
                message = result.Error
            });
        }

        return Ok(result.Data);
    }

    [HttpPut("payment-methods/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdatePaymentMethod(Guid id, [FromBody] UpdatePaymentMethodRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        var command = new Minimarket.Application.Features.Auth.Commands.UpdatePaymentMethodCommand
        {
            UserId = userId,
            PaymentMethodId = id,
            CardHolderName = request.CardHolderName,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            IsDefault = request.IsDefault
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new { 
                succeeded = false,
                errors = result.Errors,
                message = result.Error
            });
        }

        return Ok(result.Data);
    }

    [HttpDelete("payment-methods/{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePaymentMethod(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        var command = new Minimarket.Application.Features.Auth.Commands.DeletePaymentMethodCommand
        {
            UserId = userId,
            PaymentMethodId = id
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new { 
                succeeded = false,
                errors = result.Errors,
                message = result.Error
            });
        }

        return Ok(new { message = result.Data });
    }

    [HttpGet("addresses")]
    [Authorize]
    public async Task<IActionResult> GetAddresses()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        var query = new Minimarket.Application.Features.Auth.Queries.GetUserAddressesQuery
        {
            UserId = userId
        };

        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(new { 
                succeeded = false,
                errors = result.Errors,
                message = result.Error
            });
        }

        return Ok(result.Data);
    }

    [HttpPost("addresses")]
    [Authorize]
    public async Task<IActionResult> AddAddress([FromBody] AddAddressRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        var command = new Minimarket.Application.Features.Auth.Commands.AddUserAddressCommand
        {
            UserId = userId,
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
            IsDefault = request.IsDefault
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new { 
                succeeded = false,
                errors = result.Errors,
                message = result.Error
            });
        }

        return Ok(result.Data);
    }

    [HttpPut("addresses/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] UpdateAddressRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        var command = new Minimarket.Application.Features.Auth.Commands.UpdateUserAddressCommand
        {
            UserId = userId,
            AddressId = id,
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
            IsDefault = request.IsDefault
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new { 
                succeeded = false,
                errors = result.Errors,
                message = result.Error
            });
        }

        return Ok(result.Data);
    }

    [HttpDelete("addresses/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteAddress(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        var command = new Minimarket.Application.Features.Auth.Commands.DeleteUserAddressCommand
        {
            UserId = userId,
            AddressId = id
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new { 
                succeeded = false,
                errors = result.Errors,
                message = result.Error
            });
        }

        return Ok(new { message = result.Data });
    }

    public class GoogleSignInRequest
    {
        public string Credential { get; set; } = string.Empty;
    }

    private class GoogleTokenInfo
    {
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? Picture { get; set; }
        public string? Sub { get; set; }
    }

    public class AddPaymentMethodRequest
    {
        public string CardHolderName { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public bool IsDefault { get; set; } = false;
    }

    public class UpdatePaymentMethodRequest
    {
        public string CardHolderName { get; set; } = string.Empty;
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public bool IsDefault { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class AddAddressRequest
    {
        public string Label { get; set; } = string.Empty;
        public bool IsDifferentRecipient { get; set; } = false;
        public string FullName { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Dni { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Reference { get; set; }
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsDefault { get; set; } = false;
    }

    public class UpdateAddressRequest
    {
        public string Label { get; set; } = string.Empty;
        public bool IsDifferentRecipient { get; set; } = false;
        public string FullName { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Dni { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Reference { get; set; }
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsDefault { get; set; } = false;
    }
}

