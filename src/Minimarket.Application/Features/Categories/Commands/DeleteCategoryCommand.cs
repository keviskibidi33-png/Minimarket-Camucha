using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Categories.Commands;

public class DeleteCategoryCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
}

