using FluentValidation;

namespace Minimarket.Application.Features.Auth.Commands;

public class CompleteProfileCommandValidator : AbstractValidator<CompleteProfileCommand>
{
    public CompleteProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID de usuario es requerido");

        // Nombre, apellido y DNI ya están en el perfil desde el registro, no se validan aquí
        // Solo validar el teléfono
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("El teléfono es requerido")
            .MaximumLength(20).WithMessage("El teléfono no puede exceder 20 caracteres")
            .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("El formato del teléfono no es válido");
    }
}

