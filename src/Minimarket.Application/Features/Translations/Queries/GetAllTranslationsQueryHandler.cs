using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Translations.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Translations.Queries;

public class GetAllTranslationsQueryHandler : IRequestHandler<GetAllTranslationsQuery, Result<IEnumerable<TranslationDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllTranslationsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<TranslationDto>>> Handle(GetAllTranslationsQuery request, CancellationToken cancellationToken)
    {
        var translations = await _unitOfWork.Translations.GetAllAsync(cancellationToken);

        var filtered = translations.AsEnumerable();

        if (!string.IsNullOrEmpty(request.LanguageCode))
        {
            filtered = filtered.Where(t => t.LanguageCode == request.LanguageCode);
        }

        if (!string.IsNullOrEmpty(request.Category))
        {
            filtered = filtered.Where(t => t.Category == request.Category);
        }

        var result = filtered
            .OrderBy(t => t.Category)
            .ThenBy(t => t.Key)
            .Select(t => new TranslationDto
            {
                Id = t.Id,
                Key = t.Key,
                LanguageCode = t.LanguageCode,
                Value = t.Value,
                Category = t.Category,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToList();

        return Result<IEnumerable<TranslationDto>>.Success(result);
    }
}

