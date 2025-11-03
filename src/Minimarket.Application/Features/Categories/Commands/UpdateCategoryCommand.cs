using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Categories.DTOs;

namespace Minimarket.Application.Features.Categories.Commands;

public class UpdateCategoryCommand : IRequest<Result<CategoryDto>>
{
    public Guid Id { get; set; }
    public UpdateCategoryDto Category { get; set; } = new();
}

