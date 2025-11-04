using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Ofertas.Commands;

public class DeleteOfertaCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
}

