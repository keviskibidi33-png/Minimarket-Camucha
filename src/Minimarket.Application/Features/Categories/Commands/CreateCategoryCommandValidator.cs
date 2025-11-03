using FluentValidation;

namespace Minimarket.Application.Features.Categories.Commands;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Category.Name)
            .NotEmpty().WithMessage("El nombre de la categoría es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Category.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres");
    }
}

