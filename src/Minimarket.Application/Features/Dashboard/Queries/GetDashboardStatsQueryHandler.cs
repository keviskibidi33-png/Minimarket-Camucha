using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Dashboard.Queries;
using Minimarket.Application.Features.Dashboard.DTOs;
using Minimarket.Domain.Enums;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Dashboard.Queries;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDashboardStatsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DashboardStatsDto>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        // Obtener todas las ventas
        var allSales = (await _unitOfWork.Sales.GetAllAsync(cancellationToken))
            .Where(s => s.Status == SaleStatus.Pagado)
            .ToList();

        // Ventas de hoy
        var todaySales = allSales.Where(s => s.SaleDate.Date == today).ToList();
        var todayTotal = todaySales.Sum(s => s.Total);
        var todayCount = todaySales.Count;

        // Ventas del mes
        var monthSales = allSales.Where(s => s.SaleDate.Date >= monthStart && s.SaleDate.Date <= monthEnd).ToList();
        var monthTotal = monthSales.Sum(s => s.Total);
        var monthCount = monthSales.Count;

        // Productos
        var allProducts = (await _unitOfWork.Products.GetAllAsync(cancellationToken))
            .Where(p => p.IsActive)
            .ToList();
        var totalProducts = allProducts.Count;
        var lowStockProducts = allProducts.Count(p => p.Stock <= p.MinimumStock);

        // Clientes
        var totalCustomers = (await _unitOfWork.Customers.GetAllAsync(cancellationToken))
            .Count(c => c.IsActive);

        // Top productos vendidos (últimos 30 días)
        var thirtyDaysAgo = today.AddDays(-30);
        var recentSales = allSales.Where(s => s.SaleDate.Date >= thirtyDaysAgo).ToList();
        var saleDetails = (await _unitOfWork.SaleDetails.GetAllAsync(cancellationToken))
            .Where(sd => recentSales.Any(s => s.Id == sd.SaleId))
            .ToList();

        // Obtener productos para mapear nombres
        var productIds = saleDetails.Select(sd => sd.ProductId).Distinct().ToList();
        var products = (await _unitOfWork.Products.GetAllAsync(cancellationToken))
            .Where(p => productIds.Contains(p.Id))
            .ToDictionary(p => p.Id, p => p.Name);

        var topProducts = saleDetails
            .GroupBy(sd => sd.ProductId)
            .Select(g => new TopProductDto
            {
                ProductId = g.Key,
                ProductName = products.TryGetValue(g.Key, out var productName) ? productName : "Producto eliminado",
                QuantitySold = g.Sum(sd => sd.Quantity),
                TotalRevenue = g.Sum(sd => sd.Subtotal)
            })
            .OrderByDescending(p => p.QuantitySold)
            .Take(5)
            .ToList();

        // Ventas diarias (últimos 7 días)
        var dailySales = new List<DailySalesDto>();
        for (int i = 6; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var daySales = allSales.Where(s => s.SaleDate.Date == date).ToList();
            dailySales.Add(new DailySalesDto
            {
                Date = date,
                Total = daySales.Sum(s => s.Total),
                Count = daySales.Count
            });
        }

        var stats = new DashboardStatsDto
        {
            TodaySales = todayTotal,
            MonthSales = monthTotal,
            TotalProducts = totalProducts,
            LowStockProducts = lowStockProducts,
            TotalCustomers = totalCustomers,
            TodaySalesCount = todayCount,
            MonthSalesCount = monthCount,
            TopProducts = topProducts,
            DailySales = dailySales
        };

        return Result<DashboardStatsDto>.Success(stats);
    }
}

