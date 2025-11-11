using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Orders.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Orders.Queries;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, Result<PagedResult<WebOrderDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllOrdersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<WebOrderDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var allOrders = await _unitOfWork.WebOrders.GetAllAsync(cancellationToken);
        var allOrderItems = await _unitOfWork.WebOrderItems.GetAllAsync(cancellationToken);

        // Crear diccionario de items por orderId
        var itemsDict = allOrderItems
            .GroupBy(item => item.WebOrderId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Aplicar filtros
        var filteredOrders = allOrders.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            filteredOrders = filteredOrders.Where(o => o.Status == request.Status);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            filteredOrders = filteredOrders.Where(o =>
                o.OrderNumber.ToLower().Contains(searchLower) ||
                o.CustomerEmail.ToLower().Contains(searchLower) ||
                (o.CustomerName != null && o.CustomerName.ToLower().Contains(searchLower)));
        }

        if (request.StartDate.HasValue)
        {
            filteredOrders = filteredOrders.Where(o => o.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            var endDate = request.EndDate.Value.Date.AddDays(1);
            filteredOrders = filteredOrders.Where(o => o.CreatedAt < endDate);
        }

        // Ordenar por fecha de creación descendente
        var sortedOrders = filteredOrders.OrderByDescending(o => o.CreatedAt).ToList();

        // Contar total antes de paginación
        var totalCount = sortedOrders.Count;

        // Aplicar paginación
        var orders = sortedOrders
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(order =>
            {
                var orderItems = itemsDict.TryGetValue(order.Id, out var items) ? items : new List<Domain.Entities.WebOrderItem>();

                return new WebOrderDto
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
                    Items = orderItems.Select(item => new OrderItemDto
                    {
                        ProductId = item.ProductId.ToString(),
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Subtotal = item.Subtotal
                    }).ToList()
                };
            })
            .ToList();

        var pagedResult = PagedResult<WebOrderDto>.Create(orders, totalCount, request.Page, request.PageSize);

        return Result<PagedResult<WebOrderDto>>.Success(pagedResult);
    }
}

