using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Products.Commands;

public class DeleteProductCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
}

