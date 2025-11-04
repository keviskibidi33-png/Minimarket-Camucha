using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Sedes.DTOs;

namespace Minimarket.Application.Features.Sedes.Commands;

public class UpdateSedeCommand : IRequest<Result<SedeDto>>
{
    public Guid Id { get; set; }
    public UpdateSedeDto Sede { get; set; } = new();
}

