using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Translations.DTOs;

namespace Minimarket.Application.Features.Translations.Commands;

public class CreateTranslationCommand : IRequest<Result<TranslationDto>>
{
    public CreateTranslationDto Translation { get; set; } = new();
}

