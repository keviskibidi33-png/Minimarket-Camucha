namespace Minimarket.Application.Features.Dashboard.DTOs;

public class DashboardStatsDto
{
    public decimal TodaySales { get; set; }
    public decimal MonthSales { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int TotalCustomers { get; set; }
    public int TodaySalesCount { get; set; }
    public int MonthSalesCount { get; set; }
    public List<TopProductDto> TopProducts { get; set; } = new();
    public List<DailySalesDto> DailySales { get; set; } = new();
}

public class TopProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class DailySalesDto
{
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public int Count { get; set; }
}

