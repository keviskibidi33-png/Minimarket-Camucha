using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Translations.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Translations.Queries;

public class GetTranslationsByLanguageQueryHandler : IRequestHandler<GetTranslationsByLanguageQuery, Result<Dictionary<string, string>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTranslationsByLanguageQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Dictionary<string, string>>> Handle(GetTranslationsByLanguageQuery request, CancellationToken cancellationToken)
    {
        var translations = await _unitOfWork.Translations.GetAllAsync(cancellationToken);

        var filtered = translations
            .Where(t => t.LanguageCode == request.LanguageCode);

        if (!string.IsNullOrEmpty(request.Category))
        {
            filtered = filtered.Where(t => t.Category == request.Category);
        }

        var result = filtered
            .ToDictionary(t => t.Key, t => t.Value);

        return Result<Dictionary<string, string>>.Success(result);
    }
}
