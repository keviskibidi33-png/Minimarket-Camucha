using FluentValidation;

namespace Minimarket.Application.Features.Categories.Commands;

public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID de la categoría es requerido");
    }
}

