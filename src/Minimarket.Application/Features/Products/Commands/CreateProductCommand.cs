using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Products.DTOs;

namespace Minimarket.Application.Features.Products.Commands;

public class CreateProductCommand : IRequest<Result<ProductDto>>
{
    public CreateProductDto Product { get; set; } = null!;
}

