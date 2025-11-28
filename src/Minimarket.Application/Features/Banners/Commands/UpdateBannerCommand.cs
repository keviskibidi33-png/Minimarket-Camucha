using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Banners.DTOs;

namespace Minimarket.Application.Features.Banners.Commands;

public class UpdateBannerCommand : IRequest<Result<BannerDto>>
{
    public Guid Id { get; set; }
    public UpdateBannerDto Banner { get; set; } = new();
}
