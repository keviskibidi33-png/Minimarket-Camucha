using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Entities;

namespace Minimarket.Application.Features.CashClosure.Commands;

public class GenerateCashClosureCommand : IRequest<Result<GenerateCashClosureResponse>>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class GenerateCashClosureResponse
{
    public List<Sale> Sales { get; set; } = new();
    public decimal TotalPaid { get; set; }
    public int TotalCount { get; set; }
}

