using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.CashClosure.Queries;

public class GetCashClosureSummaryQuery : IRequest<Result<CashClosureSummaryDto>>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class CashClosureSummaryDto
{
    public decimal TotalPaid { get; set; }
    public int TotalCount { get; set; }
    public List<PaymentMethodSummaryDto> ByPaymentMethod { get; set; } = new();
}

public class PaymentMethodSummaryDto
{
    public string Method { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int Count { get; set; }
}

