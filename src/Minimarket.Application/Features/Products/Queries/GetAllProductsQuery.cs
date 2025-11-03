using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Products.DTOs;

namespace Minimarket.Application.Features.Products.Queries;

public class GetAllProductsQuery : IRequest<Result<PagedResult<ProductDto>>>
{
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

