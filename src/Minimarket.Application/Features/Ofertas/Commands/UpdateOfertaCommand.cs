using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Ofertas.DTOs;

namespace Minimarket.Application.Features.Ofertas.Commands;

public class UpdateOfertaCommand : IRequest<Result<OfertaDto>>
{
    public Guid Id { get; set; }
    public UpdateOfertaDto Oferta { get; set; } = new();
}

