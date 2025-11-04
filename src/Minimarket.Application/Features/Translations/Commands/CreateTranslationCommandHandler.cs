using MediatR;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Translations.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Translations.Commands;

public class CreateTranslationCommandHandler : IRequestHandler<CreateTranslationCommand, Result<TranslationDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateTranslationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TranslationDto>> Handle(CreateTranslationCommand request, CancellationToken cancellationToken)
    {
        // Verificar que no exista la combinación Key + LanguageCode + Category
        var existingTranslations = await _unitOfWork.Translations.GetAllAsync(cancellationToken);
        if (existingTranslations.Any(t => 
            t.Key == request.Translation.Key &&
            t.LanguageCode == request.Translation.LanguageCode &&
            t.Category == request.Translation.Category))
        {
            throw new BusinessRuleViolationException(
                $"Ya existe una traducción para la clave '{request.Translation.Key}' en el idioma '{request.Translation.LanguageCode}' y categoría '{request.Translation.Category}'");
        }

        var translation = new Translation
        {
            Key = request.Translation.Key,
            LanguageCode = request.Translation.LanguageCode,
            Value = request.Translation.Value,
            Category = request.Translation.Category
        };

        await _unitOfWork.Translations.AddAsync(translation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new TranslationDto
        {
            Id = translation.Id,
            Key = translation.Key,
            LanguageCode = translation.LanguageCode,
            Value = translation.Value,
            Category = translation.Category,
            CreatedAt = translation.CreatedAt,
            UpdatedAt = translation.UpdatedAt
        };

        return Result<TranslationDto>.Success(dto);
    }
}

