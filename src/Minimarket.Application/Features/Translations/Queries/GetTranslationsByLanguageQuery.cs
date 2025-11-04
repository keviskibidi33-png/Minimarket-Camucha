using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Translations.DTOs;

namespace Minimarket.Application.Features.Translations.Queries;

public class GetTranslationsByLanguageQuery : IRequest<Result<Dictionary<string, string>>>
{
    public string LanguageCode { get; set; } = "es";
    public string? Category { get; set; }
}
