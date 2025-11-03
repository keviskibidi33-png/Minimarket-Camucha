using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Categories.DTOs;

namespace Minimarket.Application.Features.Categories.Queries;

public class GetCategoryByIdQuery : IRequest<Result<CategoryDto>>
{
    public Guid Id { get; set; }
}

