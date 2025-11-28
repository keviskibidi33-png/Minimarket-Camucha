using System;
using System.Linq;
using FluentValidation;
using Minimarket.Domain.Interfaces;
using Minimarket.Domain.Specifications;

namespace Minimarket.Application.Features.Sales.Commands;

public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private const decimal IGV_RATE = 0.18m;

    public CreateSaleCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.Sale.SaleDetails)
            .NotEmpty().WithMessage("La venta debe tener al menos un producto")
            .Must(details => details.Count > 0).WithMessage("La venta debe tener al menos un producto");

        RuleForEach(x => x.Sale.SaleDetails)
            .ChildRules(detail =>
            {
                detail.RuleFor(d => d.ProductId)
                    .NotEmpty().WithMessage("El ID del producto es requerido");

                detail.RuleFor(d => d.Quantity)
                    .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0");

                detail.RuleFor(d => d.UnitPrice)
                    .GreaterThan(0).WithMessage("El precio unitario debe ser mayor a 0");
            });

        RuleFor(x => x.Sale.PaymentMethod)
            .IsInEnum().WithMessage("El método de pago no es válido");

        RuleFor(x => x.Sale.AmountPaid)
            .GreaterThanOrEqualTo(0).WithMessage("El monto pagado debe ser mayor o igual a 0");

        RuleFor(x => x.Sale.Discount)
            .GreaterThanOrEqualTo(0).WithMessage("El descuento no puede ser negativo")
            .Must((command, discount) => ValidateDiscountNotExceedsSubtotal(command))
            .WithMessage("El descuento no puede exceder el subtotal");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID del usuario es requerido");

        RuleFor(x => x.Sale)
            .Must(sale => sale.CustomerId.HasValue || sale.DocumentType == Domain.Enums.DocumentType.Boleta)
            .WithMessage("Las facturas requieren un cliente");

        // Validar existencia de productos
        RuleFor(x => x.Sale.SaleDetails)
            .MustAsync(async (details, cancellation) => await ValidateProductsExist(details, cancellation))
            .WithMessage("Uno o más productos no existen");

        // Validar stock suficiente
        RuleFor(x => x.Sale.SaleDetails)
            .MustAsync(async (details, cancellation) => await ValidateStockSufficient(details, cancellation))
            .WithMessage("Stock insuficiente para uno o más productos");

        // Validar que productos estén activos
        RuleFor(x => x.Sale.SaleDetails)
            .MustAsync(async (details, cancellation) => await ValidateProductsActive(details, cancellation))
            .WithMessage("Uno o más productos están inactivos");

        // Validar existencia de cliente si es factura
        RuleFor(x => x.Sale)
            .MustAsync(async (sale, cancellation) => await ValidateCustomerExists(sale, cancellation))
            .WithMessage("El cliente especificado no existe")
            .When(x => x.Sale.CustomerId.HasValue);

        // Validar que Factura requiere RUC válido
        RuleFor(x => x.Sale)
            .MustAsync(async (sale, cancellation) => await ValidateInvoiceRequiresRUC(sale, cancellation))
            .WithMessage("Las facturas requieren un cliente con RUC válido")
            .When(x => x.Sale.DocumentType == Domain.Enums.DocumentType.Factura);

        // Validar que Boleta puede ser DNI o Público General
        RuleFor(x => x.Sale)
            .Must(sale => sale.DocumentType == Domain.Enums.DocumentType.Boleta || 
                         (sale.CustomerId.HasValue && sale.DocumentType == Domain.Enums.DocumentType.Factura))
            .WithMessage("Las boletas pueden ser para DNI o Público General (sin cliente)")
            .When(x => x.Sale.DocumentType == Domain.Enums.DocumentType.Boleta);

        // Validar que AmountPaid >= Total
        RuleFor(x => x.Sale.AmountPaid)
            .Must((command, amountPaid) => ValidateAmountPaidSufficient(command, amountPaid))
            .WithMessage("El monto pagado debe ser mayor o igual al total");
    }

    private bool ValidateDiscountNotExceedsSubtotal(CreateSaleCommand command)
    {
        var subtotal = command.Sale.SaleDetails.Sum(d => d.Quantity * d.UnitPrice);
        // El descuento no puede exceder el subtotal y debe dejar al menos 0
        return command.Sale.Discount >= 0 && command.Sale.Discount <= subtotal;
    }

    private async Task<bool> ValidateProductsExist(List<DTOs.CreateSaleDetailDto> details, CancellationToken cancellationToken)
    {
        var productIds = details.Select(d => d.ProductId).Distinct().ToList();
        var products = (await _unitOfWork.Products.FindAsync(
            p => productIds.Contains(p.Id), 
            cancellationToken)).ToList();
        
        return products.Count == productIds.Count;
    }

    private async Task<bool> ValidateStockSufficient(List<DTOs.CreateSaleDetailDto> details, CancellationToken cancellationToken)
    {
        var productIds = details.Select(d => d.ProductId).Distinct().ToList();
        var products = (await _unitOfWork.Products.FindAsync(
            p => productIds.Contains(p.Id), 
            cancellationToken)).ToList();

        foreach (var detail in details)
        {
            var product = products.FirstOrDefault(p => p.Id == detail.ProductId);
            if (product == null) continue;

            var specification = new ProductHasSufficientStockSpecification(detail.Quantity);
            if (!specification.IsSatisfiedBy(product))
            {
                return false;
            }
        }

        return true;
    }

    private async Task<bool> ValidateProductsActive(List<DTOs.CreateSaleDetailDto> details, CancellationToken cancellationToken)
    {
        var productIds = details.Select(d => d.ProductId).Distinct().ToList();
        var products = (await _unitOfWork.Products.FindAsync(
            p => productIds.Contains(p.Id), 
            cancellationToken)).ToList();

        foreach (var product in products)
        {
            var specification = new ProductIsActiveSpecification();
            if (!specification.IsSatisfiedBy(product))
            {
                return false;
            }
        }

        return true;
    }

    private async Task<bool> ValidateCustomerExists(DTOs.CreateSaleDto sale, CancellationToken cancellationToken)
    {
        if (!sale.CustomerId.HasValue)
            return true;

        var customer = await _unitOfWork.Customers.GetByIdAsync(sale.CustomerId.Value, cancellationToken);
        return customer != null;
    }

    private bool ValidateAmountPaidSufficient(CreateSaleCommand command, decimal amountPaid)
    {
        var subtotal = command.Sale.SaleDetails.Sum(d => d.Quantity * d.UnitPrice);
        var subtotalAfterDiscount = Math.Round(subtotal - command.Sale.Discount, 2, MidpointRounding.AwayFromZero);
        var tax = Math.Round(subtotalAfterDiscount * IGV_RATE, 2, MidpointRounding.AwayFromZero);
        var total = Math.Round(subtotalAfterDiscount + tax, 2, MidpointRounding.AwayFromZero);
        
        return amountPaid >= total;
    }

    private async Task<bool> ValidateInvoiceRequiresRUC(DTOs.CreateSaleDto sale, CancellationToken cancellationToken)
    {
        // Si es Factura, debe tener un cliente con RUC
        if (sale.DocumentType != Domain.Enums.DocumentType.Factura)
            return true;

        if (!sale.CustomerId.HasValue)
            return false;

        var customer = await _unitOfWork.Customers.GetByIdAsync(sale.CustomerId.Value, cancellationToken);
        if (customer == null)
            return false;

        // Validar que el cliente tenga RUC (11 dígitos)
        return customer.DocumentType == "RUC" && 
               customer.DocumentNumber.Length == 11 && 
               customer.DocumentNumber.All(char.IsDigit);
    }
}

