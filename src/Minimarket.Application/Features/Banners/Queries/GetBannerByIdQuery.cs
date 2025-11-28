using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Banners.DTOs;

namespace Minimarket.Application.Features.Banners.Queries;

public class GetBannerByIdQuery : IRequest<Result<BannerDto>>
{
    public Guid Id { get; set; }
}

