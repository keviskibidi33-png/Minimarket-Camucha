using FluentValidation;
using System.Text.RegularExpressions;

namespace Minimarket.Application.Features.Auth.Commands;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es requerido")
            .EmailAddress().WithMessage("El correo electrónico no es válido")
            .MaximumLength(256).WithMessage("El correo electrónico no puede exceder 256 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres")
            .MaximumLength(100).WithMessage("La contraseña no puede exceder 100 caracteres");

        // Validación de DNI (requerido y debe ser válido)
        RuleFor(x => x.Dni)
            .NotEmpty().WithMessage("El DNI es requerido")
            .Must(dni => !string.IsNullOrWhiteSpace(dni) && IsValidDni(dni))
            .WithMessage("El DNI debe tener exactamente 8 dígitos numéricos");

        // Validación de nombre
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        // Validación de apellido
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es requerido")
            .MaximumLength(100).WithMessage("El apellido no puede exceder 100 caracteres");

        // Validación de teléfono
        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("El teléfono no puede exceder 20 caracteres")
            .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("El formato del teléfono no es válido")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }

    private bool IsValidDni(string dni)
    {
        if (string.IsNullOrWhiteSpace(dni))
            return false; // Requerido

        // DNI peruano: exactamente 8 dígitos numéricos
        var dniRegex = new Regex(@"^\d{8}$");
        return dniRegex.IsMatch(dni);
    }
}

