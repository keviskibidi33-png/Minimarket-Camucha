using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Orders.Commands;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOrderStatusCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.WebOrders.GetByIdAsync(request.OrderId, cancellationToken);

        if (order == null)
        {
            return Result<bool>.Failure("Pedido no encontrado");
        }

        // Validar estado
        var validStatuses = new[] { "pending", "confirmed", "preparing", "shipped", "delivered", "ready_for_pickup", "cancelled" };
        if (!validStatuses.Contains(request.Status))
        {
            return Result<bool>.Failure("Estado inválido");
        }

        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.TrackingUrl))
        {
            order.TrackingUrl = request.TrackingUrl;
        }

        if (request.EstimatedDelivery.HasValue)
        {
            order.EstimatedDelivery = request.EstimatedDelivery.Value;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}

