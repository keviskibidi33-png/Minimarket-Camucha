using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Translations.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Translations.Commands;

public class BulkCreateTranslationsCommandHandler : IRequestHandler<BulkCreateTranslationsCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public BulkCreateTranslationsCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(BulkCreateTranslationsCommand request, CancellationToken cancellationToken)
    {
        var existingTranslations = await _unitOfWork.Translations.GetAllAsync(cancellationToken);
        var existingKeys = existingTranslations
            .Select(t => $"{t.Key}_{t.LanguageCode}_{t.Category}")
            .ToHashSet();

        int created = 0;

        foreach (var translationDto in request.Translations)
        {
            var uniqueKey = $"{translationDto.Key}_{translationDto.LanguageCode}_{translationDto.Category}";
            
            if (!existingKeys.Contains(uniqueKey))
            {
                var translation = new Translation
                {
                    Key = translationDto.Key,
                    LanguageCode = translationDto.LanguageCode,
                    Value = translationDto.Value,
                    Category = translationDto.Category
                };

                await _unitOfWork.Translations.AddAsync(translation, cancellationToken);
                existingKeys.Add(uniqueKey);
                created++;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(created);
    }
}

