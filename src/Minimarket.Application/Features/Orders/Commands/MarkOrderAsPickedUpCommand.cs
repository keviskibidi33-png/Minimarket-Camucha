using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Orders.Commands;

public class MarkOrderAsPickedUpCommand : IRequest<Result<bool>>
{
    public Guid OrderId { get; set; }
    public int Rating { get; set; } // 1-5 estrellas
    public string? Comment { get; set; } // Comentario opcional
    public bool WouldRecommend { get; set; } = true; // ¿Recomendaría el servicio?
}

