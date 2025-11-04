using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Ofertas.DTOs;

namespace Minimarket.Application.Features.Ofertas.Commands;

public class CreateOfertaCommand : IRequest<Result<OfertaDto>>
{
    public CreateOfertaDto Oferta { get; set; } = new();
}

