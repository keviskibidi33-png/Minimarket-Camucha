using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Analytics.Commands;

public class TrackProductViewCommand : IRequest<Result<bool>>
{
    public Guid ProductId { get; set; }
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
