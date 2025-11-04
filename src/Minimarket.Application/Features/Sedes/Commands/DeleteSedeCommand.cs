using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Sedes.Commands;

public class DeleteSedeCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
}

