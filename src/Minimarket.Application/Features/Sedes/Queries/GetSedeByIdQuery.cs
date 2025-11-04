using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Sedes.DTOs;

namespace Minimarket.Application.Features.Sedes.Queries;

public class GetSedeByIdQuery : IRequest<Result<SedeDto>>
{
    public Guid Id { get; set; }
}

