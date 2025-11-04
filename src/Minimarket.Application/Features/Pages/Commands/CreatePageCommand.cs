using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Pages.DTOs;

namespace Minimarket.Application.Features.Pages.Commands;

public class CreatePageCommand : IRequest<Result<PageDto>>
{
    public CreatePageDto Page { get; set; } = new();
}

