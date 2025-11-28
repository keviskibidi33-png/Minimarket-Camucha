using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Banners.DTOs;

namespace Minimarket.Application.Features.Banners.Commands;

public class CreateBannerCommand : IRequest<Result<BannerDto>>
{
    public CreateBannerDto Banner { get; set; } = new();
}
