using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Products.DTOs;

namespace Minimarket.Application.Features.Products.Queries;

public class GetProductByIdQuery : IRequest<Result<ProductDto>>
{
    public Guid Id { get; set; }
}

