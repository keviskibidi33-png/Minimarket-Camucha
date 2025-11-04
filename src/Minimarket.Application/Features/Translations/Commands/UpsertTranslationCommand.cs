using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Translations.DTOs;

namespace Minimarket.Application.Features.Translations.Commands;

public class UpsertTranslationCommand : IRequest<Result<TranslationDto>>
{
    public string Key { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = "es";
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = "general";
}

