using FluentValidation;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Products.Commands;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.Product.Id)
            .NotEmpty().WithMessage("El ID del producto es requerido");

        RuleFor(x => x.Product.Code)
            .NotEmpty().WithMessage("El código del producto es requerido")
            .MaximumLength(50).WithMessage("El código no puede exceder 50 caracteres");

        RuleFor(x => x.Product.Name)
            .NotEmpty().WithMessage("El nombre del producto es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.Product.PurchasePrice)
            .GreaterThan(0).WithMessage("El precio de compra debe ser mayor a 0");

        RuleFor(x => x.Product.SalePrice)
            .GreaterThan(0).WithMessage("El precio de venta debe ser mayor a 0")
            .Must((command, salePrice) => salePrice > command.Product.PurchasePrice)
            .WithMessage("El precio de venta debe ser mayor que el precio de compra");

        RuleFor(x => x.Product.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo");

        RuleFor(x => x.Product.MinimumStock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock mínimo no puede ser negativo");

        RuleFor(x => x.Product.CategoryId)
            .NotEmpty().WithMessage("La categoría es requerida")
            .MustAsync(async (categoryId, cancellation) => await ValidateCategoryExists(categoryId, cancellation))
            .WithMessage("La categoría especificada no existe");
    }

    private async Task<bool> ValidateCategoryExists(Guid categoryId, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(categoryId, cancellationToken);
        return category != null;
    }
}

