using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Orders.Commands;

public class MarkOrderAsPickedUpCommandHandler : IRequestHandler<MarkOrderAsPickedUpCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkOrderAsPickedUpCommandHandler> _logger;

    public MarkOrderAsPickedUpCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<MarkOrderAsPickedUpCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(MarkOrderAsPickedUpCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.WebOrders.GetByIdAsync(request.OrderId, cancellationToken);

        if (order == null)
        {
            return Result<bool>.Failure("Pedido no encontrado");
        }

        // Validar que el pedido esté en estado ready_for_pickup
        if (order.Status != "ready_for_pickup")
        {
            return Result<bool>.Failure("El pedido debe estar en estado 'Listo para recoger' para marcarlo como recogido");
        }

        // Validar que el pedido sea de tipo pickup
        if (order.ShippingMethod != "pickup")
        {
            return Result<bool>.Failure("Solo los pedidos de recogida pueden marcarse como recogidos");
        }

        // Validar rating
        if (request.Rating < 1 || request.Rating > 5)
        {
            return Result<bool>.Failure("La calificación debe estar entre 1 y 5 estrellas");
        }

        // Actualizar estado del pedido
        order.Status = "picked_up";
        order.UpdatedAt = DateTime.UtcNow;

        // Crear feedback
        var feedback = new OrderFeedback
        {
            WebOrderId = order.Id,
            Rating = request.Rating,
            Comment = request.Comment,
            WouldRecommend = request.WouldRecommend
        };

        await _unitOfWork.OrderFeedbacks.AddAsync(feedback, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderNumber} marked as picked up with rating {Rating}", 
            order.OrderNumber, request.Rating);

        return Result<bool>.Success(true);
    }
}

