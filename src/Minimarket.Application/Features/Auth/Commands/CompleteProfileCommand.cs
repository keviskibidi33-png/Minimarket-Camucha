using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Auth.Commands;

public class CompleteProfileCommand : IRequest<Result<string>>
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty; // DNI peruano (8 dígitos)
    public string Phone { get; set; } = string.Empty;
    
    // Método de pago opcional (para completar perfil inicial)
    public PaymentMethodDto? PaymentMethod { get; set; }
    
    // Dirección de envío opcional (para completar perfil inicial)
    public AddressDto? Address { get; set; }
}

public class PaymentMethodDto
{
    public string CardHolderName { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty; // Se enmascarará antes de guardar
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public bool IsDefault { get; set; } = true; // Si es el primer método, será por defecto
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

