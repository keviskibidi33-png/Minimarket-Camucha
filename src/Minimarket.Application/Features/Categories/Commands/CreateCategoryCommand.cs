using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Categories.DTOs;

namespace Minimarket.Application.Features.Categories.Commands;

public class CreateCategoryCommand : IRequest<Result<CategoryDto>>
{
    public CreateCategoryDto Category { get; set; } = new();
}

