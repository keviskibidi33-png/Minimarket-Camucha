namespace Minimarket.Domain.Entities;

/// <summary>
/// Perfil extendido del usuario que almacena información adicional
/// como DNI, nombre completo, apellido y teléfono.
/// Se relaciona con IdentityUser mediante UserId.
/// </summary>
public class UserProfile : BaseEntity
{
    public Guid UserId { get; set; } // FK a IdentityUser.Id
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Dni { get; set; } // DNI peruano (8 dígitos)
    public string? Phone { get; set; }
    public bool ProfileCompleted { get; set; } = false; // Indica si el perfil está completo
    
    // Propiedades de navegación (si es necesario)
    // Nota: No podemos tener una FK directa a IdentityUser desde Domain,
    // pero podemos usar el UserId como referencia
}

