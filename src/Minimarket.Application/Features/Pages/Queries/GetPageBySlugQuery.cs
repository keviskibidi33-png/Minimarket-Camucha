using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Pages.DTOs;

namespace Minimarket.Application.Features.Pages.Queries;

public class GetPageBySlugQuery : IRequest<Result<PageDto>>
{
    public string Slug { get; set; } = string.Empty;
}

