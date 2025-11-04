using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Categories.Queries;
using Minimarket.Application.Features.Categories.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Categories.Queries;

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, Result<IEnumerable<CategoryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllCategoriesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<CategoryDto>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);

        var result = categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Orden)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                IconoUrl = c.IconoUrl,
                Orden = c.Orden,
                IsActive = c.IsActive
            })
            .ToList();

        return Result<IEnumerable<CategoryDto>>.Success(result);
    }
}

