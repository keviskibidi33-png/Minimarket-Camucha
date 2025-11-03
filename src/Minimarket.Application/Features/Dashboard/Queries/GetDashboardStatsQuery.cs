using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Dashboard.DTOs;

namespace Minimarket.Application.Features.Dashboard.Queries;

public class GetDashboardStatsQuery : IRequest<Result<DashboardStatsDto>>
{
}

