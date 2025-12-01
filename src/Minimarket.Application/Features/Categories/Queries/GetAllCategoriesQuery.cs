using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Categories.DTOs;

namespace Minimarket.Application.Features.Categories.Queries;

public class GetAllCategoriesQuery : IRequest<Result<PagedResult<CategoryDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
}

