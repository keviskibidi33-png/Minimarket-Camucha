using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.CashClosure.Queries;

public class GetCashClosureHistoryQuery : IRequest<Result<List<CashClosureHistoryDto>>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class CashClosureHistoryDto
{
    public DateTime ClosureDate { get; set; }
    public DateTime SalesStartDate { get; set; }
    public DateTime SalesEndDate { get; set; }
    public int TotalSales { get; set; }
    public decimal TotalAmount { get; set; }
    public List<PaymentMethodSummaryDto> ByPaymentMethod { get; set; } = new();
}

