using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Orders.Commands;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;

    public UpdateOrderStatusCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<UpdateOrderStatusCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.WebOrders.GetByIdAsync(request.OrderId, cancellationToken);

        if (order == null)
        {
            return Result<bool>.Failure("Pedido no encontrado");
        }

        // Validar estado
        var validStatuses = new[] { "pending", "confirmed", "preparing", "shipped", "delivered", "ready_for_pickup", "picked_up", "cancelled" };
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

        // Enviar correo de actualización de estado en segundo plano (solo para ciertos estados)
        if (request.Status == "ready_for_pickup" || request.Status == "shipped" || request.Status == "preparing")
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendOrderStatusUpdateAsync(
                        order.CustomerEmail,
                        order.CustomerName,
                        order.OrderNumber,
                        request.Status,
                        request.TrackingUrl
                    );

                    _logger.LogInformation("Order status update email sent. OrderNumber: {OrderNumber}, Status: {Status}", 
                        order.OrderNumber, request.Status);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send order status update email. OrderNumber: {OrderNumber}", 
                        order.OrderNumber);
                }
            }, cancellationToken);
        }

        return Result<bool>.Success(true);
    }
}

