using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Customers.Commands;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    // Acepta formato peruano: +51 seguido de 9 dígitos (empezando con 9) o solo 9 dígitos
    private static readonly Regex PeruvianPhoneRegex = new(@"^(\+51)?\s*9\d{8}$", RegexOptions.Compiled);

    public UpdateCustomerCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.Customer.Id)
            .NotEmpty().WithMessage("El ID del cliente es requerido");

        RuleFor(x => x.Customer.DocumentType)
            .NotEmpty().WithMessage("El tipo de documento es requerido")
            .Must(dt => dt == "DNI" || dt == "RUC").WithMessage("El tipo de documento debe ser DNI o RUC");

        RuleFor(x => x.Customer.DocumentNumber)
            .NotEmpty().WithMessage("El número de documento es requerido")
            .MaximumLength(20).WithMessage("El número de documento no puede exceder 20 caracteres")
            .Must((command, documentNumber) => 
                command.Customer.DocumentType == "DNI" 
                    ? documentNumber.Length == 8 && documentNumber.All(char.IsDigit)
                    : documentNumber.Length == 11 && documentNumber.All(char.IsDigit))
            .WithMessage(x => x.Customer.DocumentType == "DNI" 
                ? "El DNI debe tener exactamente 8 dígitos numéricos" 
                : "El RUC debe tener exactamente 11 dígitos numéricos")
            .MustAsync(async (command, documentNumber, cancellation) => 
                await ValidateDocumentUniqueness(command, documentNumber, cancellation))
            .WithMessage("Ya existe otro cliente con este documento");

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
            .WithMessage("El teléfono debe tener 9 dígitos empezando con 9. Puede incluir el prefijo +51 (formato: +51 9XXXXXXXX o 9XXXXXXXX)");

        RuleFor(x => x.Customer.Address)
            .MaximumLength(500).WithMessage("La dirección no puede exceder 500 caracteres");
    }

    private static bool ValidatePeruvianPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return true;

        // Normalizar: eliminar espacios, guiones y paréntesis, pero mantener +51 si existe
        var cleanedPhone = Regex.Replace(phone, @"[\s\-\(\)]", "");
        
        // Aceptar formato con +51 o sin él
        // Formato esperado: +519XXXXXXXX o 9XXXXXXXX (9 dígitos empezando con 9)
        return PeruvianPhoneRegex.IsMatch(cleanedPhone);
    }

    private async Task<bool> ValidateDocumentUniqueness(
        UpdateCustomerCommand command, 
        string documentNumber, 
        CancellationToken cancellationToken)
    {
        // Verificar que no exista otro cliente con el mismo documento (excluyendo el actual)
        var existingCustomer = (await _unitOfWork.Customers.FindAsync(
            c => c.DocumentType == command.Customer.DocumentType && 
                 c.DocumentNumber == documentNumber &&
                 c.Id != command.Customer.Id, // Excluir el cliente actual
            cancellationToken)).FirstOrDefault();

        return existingCustomer == null;
    }
}

