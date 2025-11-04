namespace Minimarket.Application.Features.Analytics.DTOs;

public class AnalyticsDashboardDto
{
    public int TotalPageViews { get; set; }
    public int TotalProductViews { get; set; }
    public int TotalSales { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<PageViewStatsDto> TopPages { get; set; } = new();
    public List<ProductViewStatsDto> TopProducts { get; set; } = new();
    public List<DailyStatsDto> DailyStats { get; set; } = new();
}

public class PageViewStatsDto
{
    public string PageSlug { get; set; } = string.Empty;
    public int ViewCount { get; set; }
}

public class ProductViewStatsDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int ViewCount { get; set; }
}

public class DailyStatsDto
{
    public DateTime Date { get; set; }
    public int PageViews { get; set; }
    public int ProductViews { get; set; }
    public int Sales { get; set; }
    public decimal Revenue { get; set; }
}

