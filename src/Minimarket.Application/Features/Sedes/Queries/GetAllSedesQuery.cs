using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Sedes.DTOs;

namespace Minimarket.Application.Features.Sedes.Queries;

public class GetAllSedesQuery : IRequest<Result<IEnumerable<SedeDto>>>
{
    public bool? SoloActivas { get; set; }
}

