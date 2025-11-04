using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Ofertas.DTOs;

namespace Minimarket.Application.Features.Ofertas.Queries;

public class GetAllOfertasQuery : IRequest<Result<IEnumerable<OfertaDto>>>
{
    public bool? SoloActivas { get; set; }
}

