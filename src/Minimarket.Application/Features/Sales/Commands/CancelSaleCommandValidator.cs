using FluentValidation;
using Minimarket.Domain.Interfaces;
using Minimarket.Domain.Enums;
using Minimarket.Domain.Specifications;

namespace Minimarket.Application.Features.Sales.Commands;

public class CancelSaleCommandValidator : AbstractValidator<CancelSaleCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CancelSaleCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.SaleId)
            .NotEmpty().WithMessage("El ID de la venta es requerido");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("La razón de anulación es requerida")
            .MaximumLength(500).WithMessage("La razón no puede exceder 500 caracteres");

        // Validar que la venta existe
        RuleFor(x => x.SaleId)
            .MustAsync(async (saleId, cancellation) => await ValidateSaleExists(saleId, cancellation))
            .WithMessage("La venta especificada no existe");

        // Validar transiciones de estado válidas
        RuleFor(x => x.SaleId)
            .MustAsync(async (saleId, cancellation) => await ValidateSaleCanBeCancelled(saleId, cancellation))
            .WithMessage("La venta ya está anulada o no puede ser anulada");
    }

    private async Task<bool> ValidateSaleExists(Guid saleId, CancellationToken cancellationToken)
    {
        var sale = await _unitOfWork.Sales.GetByIdAsync(saleId, cancellationToken);
        return sale != null;
    }

    private async Task<bool> ValidateSaleCanBeCancelled(Guid saleId, CancellationToken cancellationToken)
    {
        var sale = await _unitOfWork.Sales.GetByIdAsync(saleId, cancellationToken);
        
        if (sale == null)
            return false;

        var specification = new SaleCanBeCancelledSpecification();
        return specification.IsSatisfiedBy(sale);
    }
}

