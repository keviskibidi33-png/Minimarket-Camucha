using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Banners.Commands;

public class IncrementBannerViewCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
}

