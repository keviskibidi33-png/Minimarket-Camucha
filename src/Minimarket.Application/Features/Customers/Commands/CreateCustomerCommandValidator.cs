using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Customers.Commands;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private static readonly Regex PeruvianPhoneRegex = new(@"^9\d{8}$", RegexOptions.Compiled);

    public CreateCustomerCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.Customer.DocumentType)
            .NotEmpty().WithMessage("El tipo de documento es requerido")
            .Must(dt => dt == "DNI" || dt == "RUC").WithMessage("El tipo de documento debe ser DNI o RUC");

        RuleFor(x => x.Customer.DocumentNumber)
            .NotEmpty().WithMessage("El número de documento es requerido")
            .MaximumLength(20).WithMessage("El número de documento no puede exceder 20 caracteres")
            .When(x => x.Customer.DocumentType == "DNI", ApplyConditionTo.CurrentValidator)
            .Must(dn => dn.Length == 8 && dn.All(char.IsDigit)).WithMessage("El DNI debe tener 8 dígitos")
            .When(x => x.Customer.DocumentType == "DNI", ApplyConditionTo.CurrentValidator)
            .Must(dn => dn.Length == 11 && dn.All(char.IsDigit)).WithMessage("El RUC debe tener 11 dígitos")
            .When(x => x.Customer.DocumentType == "RUC", ApplyConditionTo.CurrentValidator)
            .MustAsync(async (command, documentNumber, cancellation) => 
                await ValidateDocumentUniqueness(command, documentNumber, cancellation))
            .WithMessage("Ya existe un cliente con este documento");

        RuleFor(x => x.Customer.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.Customer.Email)
            .EmailAddress().WithMessage("El email no es válido")
            .When(x => !string.IsNullOrWhiteSpace(x.Customer.Email))
            .MaximumLength(100).WithMessage("El email no puede exceder 100 caracteres");

        RuleFor(x => x.Customer.Phone)
            .MaximumLength(20).WithMessage("El teléfono no puede exceder 20 caracteres")
            .Must(phone => string.IsNullOrWhiteSpace(phone) || ValidatePeruvianPhone(phone))
            .WithMessage("El teléfono debe tener 9 dígitos y empezar con 9 (formato peruano)");

        RuleFor(x => x.Customer.Address)
            .MaximumLength(500).WithMessage("La dirección no puede exceder 500 caracteres");
    }

    private static bool ValidatePeruvianPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return true;

        // Eliminar espacios, guiones y otros caracteres para validar solo dígitos
        var cleanedPhone = Regex.Replace(phone, @"[\s\-\(\)]", "");
        
        return PeruvianPhoneRegex.IsMatch(cleanedPhone);
    }

    private async Task<bool> ValidateDocumentUniqueness(
        CreateCustomerCommand command, 
        string documentNumber, 
        CancellationToken cancellationToken)
    {
        var existingCustomer = (await _unitOfWork.Customers.FindAsync(
            c => c.DocumentType == command.Customer.DocumentType && 
                 c.DocumentNumber == documentNumber,
            cancellationToken)).FirstOrDefault();

        return existingCustomer == null;
    }
}

