namespace Minimarket.Application.Features.Sales.DTOs;

public class CreateSaleDetailDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

