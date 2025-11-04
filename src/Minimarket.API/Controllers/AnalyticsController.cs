using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minimarket.Application.Features.Analytics.Commands;
using Minimarket.Application.Features.Analytics.Queries;
using System.Security.Claims;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnalyticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("dashboard")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetDashboard([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var query = new GetAnalyticsDashboardQuery
        {
            StartDate = startDate,
            EndDate = endDate
        };
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpPost("track/page-view")]
    [AllowAnonymous] // Tracking público
    public async Task<IActionResult> TrackPageView([FromBody] TrackPageViewCommand command)
    {
        // Obtener información del request
        command.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        command.UserAgent = Request.Headers["User-Agent"].ToString();

        // Obtener userId si está autenticado
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null)
        {
            command.UserId = userIdClaim.Value;
        }

        var result = await _mediator.Send(command);
        return Ok(result.Data);
    }

    [HttpPost("track/product-view")]
    [AllowAnonymous] // Tracking público
    public async Task<IActionResult> TrackProductView([FromBody] TrackProductViewCommand command)
    {
        command.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        command.UserAgent = Request.Headers["User-Agent"].ToString();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null)
        {
            command.UserId = userIdClaim.Value;
        }

        var result = await _mediator.Send(command);
        return Ok(result.Data);
    }
}
