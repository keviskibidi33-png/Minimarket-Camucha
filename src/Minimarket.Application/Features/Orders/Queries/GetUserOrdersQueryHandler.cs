using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Orders.DTOs;
using Minimarket.Application.Features.Orders.Queries;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Orders.Queries;

public class GetUserOrdersQueryHandler : IRequestHandler<GetUserOrdersQuery, Result<List<WebOrderDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetUserOrdersQueryHandler> _logger;

    public GetUserOrdersQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetUserOrdersQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<List<WebOrderDto>>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var orders = await _unitOfWork.WebOrders.GetAllAsync(cancellationToken);
            
            // Filtrar por email del usuario
            var userOrders = orders
                .Where(o => o.CustomerEmail.Equals(request.UserEmail, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            var dtos = userOrders.Select(order => new WebOrderDto
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
                TrackingUrl = order.TrackingUrl,
                EstimatedDelivery = order.EstimatedDelivery,
                PaymentProofUrl = order.PaymentProofUrl,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Items = order.OrderItems.Select(item => new OrderItemDto
                {
                    ProductId = item.ProductId.ToString(),
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Subtotal = item.Subtotal
                }).ToList()
            }).ToList();

            _logger.LogInformation("Retrieved {Count} orders for user {Email}", dtos.Count, request.UserEmail);

            return Result<List<WebOrderDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for user {Email}", request.UserEmail);
            return Result<List<WebOrderDto>>.Failure("Error al obtener los pedidos");
        }
    }
}

