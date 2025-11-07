using FluentValidation;
using System.Text.RegularExpressions;

namespace Minimarket.Application.Features.Auth.Commands;

public class CompleteProfileCommandValidator : AbstractValidator<CompleteProfileCommand>
{
    public CompleteProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID de usuario es requerido");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es requerido")
            .MaximumLength(100).WithMessage("El apellido no puede exceder 100 caracteres");

        RuleFor(x => x.Dni)
            .NotEmpty().WithMessage("El DNI es requerido")
            .Must(IsValidDni).WithMessage("El DNI debe tener exactamente 8 dígitos numéricos");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("El teléfono es requerido")
            .MaximumLength(20).WithMessage("El teléfono no puede exceder 20 caracteres")
            .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("El formato del teléfono no es válido");
    }

    private bool IsValidDni(string dni)
    {
        if (string.IsNullOrWhiteSpace(dni))
            return false;

        // DNI peruano: exactamente 8 dígitos numéricos
        var dniRegex = new Regex(@"^\d{8}$");
        return dniRegex.IsMatch(dni);
    }
}

