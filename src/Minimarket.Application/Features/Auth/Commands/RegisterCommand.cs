using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Auth.DTOs;

namespace Minimarket.Application.Features.Auth.Commands;

public class RegisterCommand : IRequest<Result<LoginResponse>>
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    
    // Campos adicionales del perfil
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Dni { get; set; } // DNI peruano (8 dígitos)
    public string? Phone { get; set; }
}

