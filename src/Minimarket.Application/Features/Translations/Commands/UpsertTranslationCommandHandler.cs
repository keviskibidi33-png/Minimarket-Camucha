using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Translations.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Translations.Commands;

public class UpsertTranslationCommandHandler : IRequestHandler<UpsertTranslationCommand, Result<TranslationDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpsertTranslationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TranslationDto>> Handle(UpsertTranslationCommand request, CancellationToken cancellationToken)
    {
        var translations = await _unitOfWork.Translations.GetAllAsync(cancellationToken);

        var existing = translations.FirstOrDefault(t =>
            t.Key == request.Key &&
            t.LanguageCode == request.LanguageCode &&
            t.Category == request.Category);

        if (existing != null)
        {
            // Actualizar
            existing.Value = request.Value;
            existing.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Translations.UpdateAsync(existing, cancellationToken);
        }
        else
        {
            // Crear nuevo
            existing = new Translation
            {
                Key = request.Key,
                LanguageCode = request.LanguageCode,
                Value = request.Value,
                Category = request.Category
            };

            await _unitOfWork.Translations.AddAsync(existing, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new TranslationDto
        {
            Id = existing.Id,
            Key = existing.Key,
            LanguageCode = existing.LanguageCode,
            Value = existing.Value,
            Category = existing.Category,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = existing.UpdatedAt
        };

        return Result<TranslationDto>.Success(dto);
    }
}

