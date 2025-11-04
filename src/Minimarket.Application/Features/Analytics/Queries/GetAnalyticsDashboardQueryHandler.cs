using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Analytics.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Analytics.Queries;

public class GetAnalyticsDashboardQueryHandler : IRequestHandler<GetAnalyticsDashboardQuery, Result<AnalyticsDashboardDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAnalyticsDashboardQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AnalyticsDashboardDto>> Handle(GetAnalyticsDashboardQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-30);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        // Obtener datos
        var pageViews = await _unitOfWork.PageViews.GetAllAsync(cancellationToken);
        var productViews = await _unitOfWork.ProductViews.GetAllAsync(cancellationToken);
        var sales = await _unitOfWork.Sales.GetAllAsync(cancellationToken);

        // Filtrar por fecha
        var filteredPageViews = pageViews.Where(pv => pv.ViewedAt >= startDate && pv.ViewedAt <= endDate).ToList();
        var filteredProductViews = productViews.Where(pv => pv.ViewedAt >= startDate && pv.ViewedAt <= endDate).ToList();
        var filteredSales = sales.Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate).ToList();

        // Top páginas
        var topPages = filteredPageViews
            .GroupBy(pv => pv.PageSlug)
            .Select(g => new PageViewStatsDto
            {
                PageSlug = g.Key,
                ViewCount = g.Count()
            })
            .OrderByDescending(p => p.ViewCount)
            .Take(10)
            .ToList();

        // Top productos
        var products = await _unitOfWork.Products.GetAllAsync(cancellationToken);
        var productsDict = products.ToDictionary(p => p.Id, p => p.Name);

        var topProducts = filteredProductViews
            .GroupBy(pv => pv.ProductId)
            .Select(g => new ProductViewStatsDto
            {
                ProductId = g.Key,
                ProductName = productsDict.TryGetValue(g.Key, out var name) ? name : "Producto eliminado",
                ViewCount = g.Count()
            })
            .OrderByDescending(p => p.ViewCount)
            .Take(10)
            .ToList();

        // Estadísticas diarias
        var dailyStats = new List<DailyStatsDto>();
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var dayPageViews = filteredPageViews.Count(pv => pv.ViewedAt.Date == date);
            var dayProductViews = filteredProductViews.Count(pv => pv.ViewedAt.Date == date);
            var daySales = filteredSales.Where(s => s.SaleDate.Date == date).ToList();
            var dayRevenue = daySales.Sum(s => s.Total);

            dailyStats.Add(new DailyStatsDto
            {
                Date = date,
                PageViews = dayPageViews,
                ProductViews = dayProductViews,
                Sales = daySales.Count,
                Revenue = dayRevenue
            });
        }

        var result = new AnalyticsDashboardDto
        {
            TotalPageViews = filteredPageViews.Count,
            TotalProductViews = filteredProductViews.Count,
            TotalSales = filteredSales.Count,
            TotalRevenue = filteredSales.Sum(s => s.Total),
            TopPages = topPages,
            TopProducts = topProducts,
            DailyStats = dailyStats
        };

        return Result<AnalyticsDashboardDto>.Success(result);
    }
}
