using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Analytics.DTOs;

namespace Minimarket.Application.Features.Analytics.Queries;

public class GetAnalyticsDashboardQuery : IRequest<Result<AnalyticsDashboardDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

