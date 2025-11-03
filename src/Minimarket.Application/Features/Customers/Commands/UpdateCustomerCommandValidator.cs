using FluentValidation;

namespace Minimarket.Application.Features.Customers.Commands;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Customer.Id)
            .NotEmpty().WithMessage("El ID del cliente es requerido");

        RuleFor(x => x.Customer.DocumentType)
            .NotEmpty().WithMessage("El tipo de documento es requerido")
            .Must(dt => dt == "DNI" || dt == "RUC").WithMessage("El tipo de documento debe ser DNI o RUC");

        RuleFor(x => x.Customer.DocumentNumber)
            .NotEmpty().WithMessage("El número de documento es requerido")
            .MaximumLength(20).WithMessage("El número de documento no puede exceder 20 caracteres");

        RuleFor(x => x.Customer.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.Customer.Email)
            .EmailAddress().WithMessage("El email no es válido")
            .When(x => !string.IsNullOrWhiteSpace(x.Customer.Email))
            .MaximumLength(100).WithMessage("El email no puede exceder 100 caracteres");
    }
}

