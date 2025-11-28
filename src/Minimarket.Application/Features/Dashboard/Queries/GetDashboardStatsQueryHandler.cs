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

        // OPTIMIZACIÓN: Usar consultas específicas en lugar de cargar todo en memoria
        // Ventas de hoy (consulta optimizada)
        var todaySales = (await _unitOfWork.SaleRepository.GetByDateRangeAsync(today, today.AddDays(1).AddTicks(-1), cancellationToken))
            .Where(s => s.Status == SaleStatus.Pagado)
            .ToList();
        var todayTotal = todaySales.Sum(s => s.Total);
        var todayCount = todaySales.Count;

        // Ventas del mes (consulta optimizada)
        var monthSales = (await _unitOfWork.SaleRepository.GetByDateRangeAsync(monthStart, monthEnd, cancellationToken))
            .Where(s => s.Status == SaleStatus.Pagado)
            .ToList();
        var monthTotal = monthSales.Sum(s => s.Total);
        var monthCount = monthSales.Count;

        // Productos (consulta optimizada: solo activos)
        var activeProducts = (await _unitOfWork.Products.FindAsync(p => p.IsActive, cancellationToken)).ToList();
        var totalProducts = activeProducts.Count;
        var lowStockProducts = activeProducts.Count(p => p.Stock <= p.MinimumStock);

        // Clientes (consulta optimizada: solo activos)
        var activeCustomers = (await _unitOfWork.Customers.FindAsync(c => c.IsActive, cancellationToken)).ToList();
        var totalCustomers = activeCustomers.Count;

        // Top productos vendidos (últimos 30 días) - consulta optimizada
        var thirtyDaysAgo = today.AddDays(-30);
        var recentSales = (await _unitOfWork.SaleRepository.GetByDateRangeAsync(thirtyDaysAgo, today.AddDays(1).AddTicks(-1), cancellationToken))
            .Where(s => s.Status == SaleStatus.Pagado)
            .ToList();
        
        // Obtener detalles de ventas recientes (solo IDs de ventas)
        var recentSaleIds = recentSales.Select(s => s.Id).ToList();
        var saleDetails = (await _unitOfWork.SaleDetails.FindAsync(sd => recentSaleIds.Contains(sd.SaleId), cancellationToken)).ToList();

        // Obtener productos para mapear nombres (solo los que se vendieron)
        var productIds = saleDetails.Select(sd => sd.ProductId).Distinct().ToList();
        var products = (await _unitOfWork.Products.FindAsync(p => productIds.Contains(p.Id), cancellationToken))
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

        // Ventas diarias (últimos 7 días) - consultas optimizadas por día
        var dailySales = new List<DailySalesDto>();
        for (int i = 6; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var dayStart = date;
            var dayEnd = date.AddDays(1).AddTicks(-1);
            var daySales = (await _unitOfWork.SaleRepository.GetByDateRangeAsync(dayStart, dayEnd, cancellationToken))
                .Where(s => s.Status == SaleStatus.Pagado)
                .ToList();
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

