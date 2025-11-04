using MediatR;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Categories.Queries;
using Minimarket.Application.Features.Categories.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Categories.Queries;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCategoryByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Id, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException("Category", request.Id);
        }

        var categoryDto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            IconoUrl = category.IconoUrl,
            Orden = category.Orden,
            IsActive = category.IsActive
        };

        return Result<CategoryDto>.Success(categoryDto);
    }
}

