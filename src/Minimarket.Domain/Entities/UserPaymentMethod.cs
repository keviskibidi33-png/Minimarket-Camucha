namespace Minimarket.Domain.Entities;

/// <summary>
/// Método de pago guardado por un usuario (tarjetas de crédito/débito)
/// </summary>
public class UserPaymentMethod : BaseEntity
{
    public Guid UserId { get; set; } // FK a IdentityUser.Id
    public string CardHolderName { get; set; } = string.Empty; // Nombre del titular
    public string CardNumberMasked { get; set; } = string.Empty; // Últimos 4 dígitos: "**** **** **** 1234"
    public string CardType { get; set; } = string.Empty; // "Visa", "Mastercard", "Amex", etc.
    public int ExpiryMonth { get; set; } // Mes de expiración (1-12)
    public int ExpiryYear { get; set; } // Año de expiración (ej: 2025)
    public bool IsDefault { get; set; } = false; // Si es el método de pago por defecto
    public string? Last4Digits { get; set; } // Últimos 4 dígitos para referencia rápida
}

