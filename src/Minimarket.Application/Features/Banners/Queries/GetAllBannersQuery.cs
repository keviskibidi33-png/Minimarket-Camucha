using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Banners.DTOs;

namespace Minimarket.Application.Features.Banners.Queries;

public class GetAllBannersQuery : IRequest<Result<List<BannerDto>>>
{
    public bool? SoloActivos { get; set; }
    public int? Tipo { get; set; }
    public int? Posicion { get; set; }
}
