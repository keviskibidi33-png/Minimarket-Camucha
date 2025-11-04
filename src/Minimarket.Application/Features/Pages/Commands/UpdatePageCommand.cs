using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Pages.DTOs;

namespace Minimarket.Application.Features.Pages.Commands;

public class UpdatePageCommand : IRequest<Result<PageDto>>
{
    public Guid Id { get; set; }
    public UpdatePageDto Page { get; set; } = new();
}

