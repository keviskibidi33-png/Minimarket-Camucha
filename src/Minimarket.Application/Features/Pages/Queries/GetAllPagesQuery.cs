using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Pages.DTOs;

namespace Minimarket.Application.Features.Pages.Queries;

public class GetAllPagesQuery : IRequest<Result<IEnumerable<PageDto>>>
{
    public bool? SoloActivas { get; set; }
}

