namespace Minimarket.Domain.Entities;

public class OrderFeedback : BaseEntity
{
    public Guid WebOrderId { get; set; }
    public int Rating { get; set; } // 1-5 estrellas
    public string? Comment { get; set; } // Comentario opcional
    public bool WouldRecommend { get; set; } // ¿Recomendaría el servicio?
    
    // Navigation property
    public virtual WebOrder? WebOrder { get; set; }
}

