using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Categories.DTOs;

namespace Minimarket.Application.Features.Categories.Queries;

public class GetAllCategoriesQuery : IRequest<Result<IEnumerable<CategoryDto>>>
{
}

