using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Auth.Commands;

public class CompleteProfileCommand : IRequest<Result<string>>
{
    public Guid UserId { get; set; }
    public string Phone { get; set; } = string.Empty;
    // Nombre, apellido y DNI ya están en el perfil desde el registro
    
    // Dirección de envío opcional (para completar perfil inicial)
    public AddressDto? Address { get; set; }
}

public class AddressDto
{
    public string Label { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string District { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsDefault { get; set; } = true; // Si es la primera dirección, será por defecto
}

