using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Orders.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Orders.Queries;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<WebOrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetOrderByIdQueryHandler> _logger;

    public GetOrderByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetOrderByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<WebOrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validar entrada
            if (request.OrderId == Guid.Empty)
            {
                _logger.LogWarning("Invalid OrderId provided: {OrderId}", request.OrderId);
                return Result<WebOrderDto>.Failure("ID de pedido inválido");
            }

            _logger.LogInformation("Retrieving order {OrderId}", request.OrderId);

            // Buscar el pedido
            var order = await _unitOfWork.WebOrders.FirstOrDefaultAsync(
                o => o.Id == request.OrderId, 
                cancellationToken);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", request.OrderId);
                return Result<WebOrderDto>.Failure("Pedido no encontrado");
            }

            // Cargar los items del pedido si no están cargados
            if (order.OrderItems == null || !order.OrderItems.Any())
            {
                _logger.LogInformation("Loading order items for order {OrderId}", request.OrderId);
                var orderItems = await _unitOfWork.WebOrderItems.FindAsync(
                    item => item.WebOrderId == order.Id, 
                    cancellationToken);
                order.OrderItems = orderItems.ToList();
            }

            _logger.LogInformation(
                "Building DTO for order {OrderId} - CustomerEmail: {Email}, OrderNumber: {OrderNumber}, ItemsCount: {Count}", 
                request.OrderId, 
                order.CustomerEmail, 
                order.OrderNumber,
                order.OrderItems?.Count ?? 0);

            // Construir el DTO con validaciones
            var dto = new WebOrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber ?? string.Empty,
                CustomerEmail = order.CustomerEmail ?? string.Empty,
                CustomerName = order.CustomerName ?? string.Empty,
                CustomerPhone = order.CustomerPhone,
                ShippingMethod = order.ShippingMethod ?? string.Empty,
                ShippingAddress = order.ShippingAddress,
                ShippingCity = order.ShippingCity,
                ShippingRegion = order.ShippingRegion,
                SelectedSedeId = order.SelectedSedeId?.ToString(),
                PaymentMethod = order.PaymentMethod ?? string.Empty,
                WalletMethod = order.WalletMethod,
                RequiresPaymentProof = order.RequiresPaymentProof,
                Status = order.Status ?? string.Empty,
                Subtotal = order.Subtotal,
                ShippingCost = order.ShippingCost,
                Total = order.Total,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Items = order.OrderItems?.Select(item => new OrderItemDto
                {
                    ProductId = item.ProductId.ToString(),
                    ProductName = item.ProductName ?? string.Empty,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Subtotal = item.Subtotal
                }).ToList() ?? new List<OrderItemDto>()
            };

            // Validar que el DTO tenga datos mínimos
            if (dto.Id == Guid.Empty)
            {
                _logger.LogError("DTO built with invalid ID for order {OrderId}", request.OrderId);
                return Result<WebOrderDto>.Failure("Error al construir los datos del pedido");
            }

            _logger.LogInformation(
                "DTO built successfully - Id: {Id}, OrderNumber: {OrderNumber}, CustomerEmail: {Email}, ItemsCount: {Count}, Total: {Total}", 
                dto.Id, 
                dto.OrderNumber, 
                dto.CustomerEmail, 
                dto.Items?.Count ?? 0,
                dto.Total);

            return Result<WebOrderDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", request.OrderId);
            return Result<WebOrderDto>.Failure("Error al obtener el pedido");
        }
    }
}

