using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Analytics.Commands;

public class TrackPageViewCommand : IRequest<Result<bool>>
{
    public string PageSlug { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
