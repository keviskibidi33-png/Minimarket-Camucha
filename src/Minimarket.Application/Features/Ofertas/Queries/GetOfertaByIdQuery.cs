using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Ofertas.DTOs;

namespace Minimarket.Application.Features.Ofertas.Queries;

public class GetOfertaByIdQuery : IRequest<Result<OfertaDto>>
{
    public Guid Id { get; set; }
}

