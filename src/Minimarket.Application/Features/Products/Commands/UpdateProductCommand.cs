using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Products.DTOs;

namespace Minimarket.Application.Features.Products.Commands;

public class UpdateProductCommand : IRequest<Result<ProductDto>>
{
    public UpdateProductDto Product { get; set; } = null!;
}

