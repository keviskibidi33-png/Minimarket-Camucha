using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Sedes.DTOs;

namespace Minimarket.Application.Features.Sedes.Commands;

public class CreateSedeCommand : IRequest<Result<SedeDto>>
{
    public CreateSedeDto Sede { get; set; } = new();
}

