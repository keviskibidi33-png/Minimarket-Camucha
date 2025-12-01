using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Orders.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Orders.Commands;

public class CreateWebOrderCommandHandler : IRequestHandler<CreateWebOrderCommand, Result<WebOrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<CreateWebOrderCommandHandler> _logger;

    public CreateWebOrderCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<CreateWebOrderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<WebOrderDto>> Handle(CreateWebOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Calcular fecha estimada de entrega
            DateTime? estimatedDelivery = null;
            if (request.Order.ShippingMethod == "delivery")
            {
                // Obtener días de entrega desde configuración
                var deliveryDaysSetting = await _unitOfWork.SystemSettings.GetAllAsync(cancellationToken);
                var deliveryDays = 3; // Por defecto
                var setting = deliveryDaysSetting.FirstOrDefault(s => s.Key == "delivery_days");
                if (setting != null && int.TryParse(setting.Value, out var days))
                {
                    deliveryDays = days;
                }
                estimatedDelivery = DateTime.UtcNow.AddDays(deliveryDays);
            }
            else if (request.Order.ShippingMethod == "pickup")
            {
                // Obtener días para pickup desde configuración
                var pickupDaysSetting = await _unitOfWork.SystemSettings.GetAllAsync(cancellationToken);
                var pickupDays = 2; // Por defecto
                var setting = pickupDaysSetting.FirstOrDefault(s => s.Key == "pickup_days");
                if (setting != null && int.TryParse(setting.Value, out var days))
                {
                    pickupDays = days;
                }
                estimatedDelivery = DateTime.UtcNow.AddDays(pickupDays);
            }

            // Crear pedido
            var order = new WebOrder
            {
                OrderNumber = request.Order.OrderNumber,
                CustomerEmail = request.Order.CustomerEmail,
                CustomerName = request.Order.CustomerName,
                CustomerPhone = request.Order.CustomerPhone,
                ShippingMethod = request.Order.ShippingMethod,
                ShippingAddress = request.Order.ShippingAddress,
                ShippingCity = request.Order.ShippingCity,
                ShippingRegion = request.Order.ShippingRegion,
                SelectedSedeId = !string.IsNullOrWhiteSpace(request.Order.SelectedSedeId) 
                    && Guid.TryParse(request.Order.SelectedSedeId, out var sedeId) 
                    ? sedeId 
                    : null,
                PaymentMethod = request.Order.PaymentMethod,
                WalletMethod = request.Order.WalletMethod,
                RequiresPaymentProof = request.Order.RequiresPaymentProof,
                // Todos los pedidos empiezan como "pending" hasta que el admin los apruebe
                // Esto permite revisar y validar todos los pedidos antes de confirmarlos
                Status = "pending",
                Subtotal = request.Order.Subtotal,
                ShippingCost = request.Order.ShippingCost,
                Total = request.Order.Total,
                EstimatedDelivery = estimatedDelivery
            };

            await _unitOfWork.WebOrders.AddAsync(order, cancellationToken);

            // Crear items del pedido
            foreach (var itemDto in request.Order.Items)
            {
                // Validar que ProductId sea un GUID válido
                if (string.IsNullOrWhiteSpace(itemDto.ProductId) || !Guid.TryParse(itemDto.ProductId, out var productId))
                {
                    _logger.LogWarning("Invalid ProductId in order item: {ProductId}, ProductName: {ProductName}", 
                        itemDto.ProductId, itemDto.ProductName);
                    throw new ArgumentException($"Invalid ProductId format: {itemDto.ProductId}");
                }

                var orderItem = new WebOrderItem
                {
                    WebOrderId = order.Id,
                    ProductId = productId,
                    ProductName = itemDto.ProductName,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    Subtotal = itemDto.Subtotal
                };
                await _unitOfWork.WebOrderItems.AddAsync(orderItem, cancellationToken);
            }

            // Guardar todo en una sola operación (Entity Framework maneja la transacción automáticamente)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Construir DTO de respuesta PRIMERO para devolver respuesta rápida
            var dto = new WebOrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerEmail = order.CustomerEmail,
                CustomerName = order.CustomerName,
                CustomerPhone = order.CustomerPhone,
                ShippingMethod = order.ShippingMethod,
                ShippingAddress = order.ShippingAddress,
                ShippingCity = order.ShippingCity,
                ShippingRegion = order.ShippingRegion,
                SelectedSedeId = order.SelectedSedeId?.ToString(),
                PaymentMethod = order.PaymentMethod,
                WalletMethod = order.WalletMethod,
                RequiresPaymentProof = order.RequiresPaymentProof,
                Status = order.Status,
                Subtotal = order.Subtotal,
                ShippingCost = order.ShippingCost,
                Total = order.Total,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Items = request.Order.Items.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Subtotal = i.Subtotal
                }).ToList()
            };

            _logger.LogInformation("Web order created successfully. OrderNumber: {OrderNumber}, Total: {Total}", 
                order.OrderNumber, order.Total);

            // Enviar correo de confirmación en segundo plano (fire-and-forget)
            // Esto no bloquea la respuesta al cliente
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendOrderConfirmationAsync(
                        order.CustomerEmail,
                        order.CustomerName,
                        order.OrderNumber,
                        order.Total,
                        order.ShippingMethod,
                        estimatedDelivery
                    );
                    _logger.LogInformation("Order confirmation email sent. OrderNumber: {OrderNumber}, Email: {Email}", 
                        order.OrderNumber, order.CustomerEmail);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send order confirmation email. OrderNumber: {OrderNumber}", 
                        order.OrderNumber);
                    // No fallar el proceso si el correo falla
                }
            }, cancellationToken);

            return Result<WebOrderDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating web order");
            throw;
        }
    }
}

