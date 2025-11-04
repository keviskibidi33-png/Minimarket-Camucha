using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Translations.DTOs;

namespace Minimarket.Application.Features.Translations.Commands;

public class BulkCreateTranslationsCommand : IRequest<Result<int>>
{
    public List<CreateTranslationDto> Translations { get; set; } = new();
}

