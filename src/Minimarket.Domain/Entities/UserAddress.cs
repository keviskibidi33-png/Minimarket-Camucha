namespace Minimarket.Domain.Entities;

public class UserAddress : BaseEntity
{
    public Guid UserId { get; set; }
    public string Label { get; set; } = string.Empty; // Ej: "Casa", "Trabajo", "Oficina"
    public bool IsDifferentRecipient { get; set; } = false; // Si es para otra persona
    public string FullName { get; set; } = string.Empty; // Nombre completo del destinatario
    public string? FirstName { get; set; } // Nombre del destinatario (si es diferente)
    public string? LastName { get; set; } // Apellido del destinatario (si es diferente)
    public string? Dni { get; set; } // DNI del destinatario (si es diferente)
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty; // Dirección completa
    public string? Reference { get; set; } // Referencia (ej: "Al lado del banco")
    public string District { get; set; } = string.Empty; // Distrito
    public string City { get; set; } = string.Empty; // Ciudad (Lima, etc.)
    public string Region { get; set; } = string.Empty; // Región/Departamento
    public string? PostalCode { get; set; }
    public decimal? Latitude { get; set; } // Coordenada para mapas
    public decimal? Longitude { get; set; } // Coordenada para mapas
    public bool IsDefault { get; set; } = false; // Dirección por defecto
}

