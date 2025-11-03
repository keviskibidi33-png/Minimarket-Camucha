using FluentValidation;

namespace Minimarket.Application.Features.Sales.Commands;

public class SendSaleReceiptCommandValidator : AbstractValidator<SendSaleReceiptCommand>
{
    public SendSaleReceiptCommandValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty().WithMessage("El ID de la venta es requerido");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es requerido")
            .EmailAddress().WithMessage("El formato del correo electrónico no es válido");

        RuleFor(x => x.DocumentType)
            .NotEmpty().WithMessage("El tipo de documento es requerido")
            .Must(dt => dt == "Boleta" || dt == "Factura").WithMessage("El tipo de documento debe ser Boleta o Factura");
    }
}

