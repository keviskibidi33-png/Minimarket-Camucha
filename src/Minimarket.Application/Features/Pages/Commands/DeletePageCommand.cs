using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Pages.Commands;

public class DeletePageCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
}

