using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Translations.DTOs;

namespace Minimarket.Application.Features.Translations.Queries;

public class GetAllTranslationsQuery : IRequest<Result<IEnumerable<TranslationDto>>>
{
    public string? LanguageCode { get; set; }
    public string? Category { get; set; }
}

