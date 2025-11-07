using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Auth.Commands;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProfileCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        // Buscar el perfil del usuario
        var profile = await _unitOfWork.UserProfiles.FirstOrDefaultAsync(
            up => up.UserId == request.UserId, cancellationToken);

        if (profile == null)
        {
            return Result<string>.Failure("Perfil de usuario no encontrado");
        }

        // Actualizar datos del perfil (DNI no se actualiza porque no se puede modificar)
        profile.FirstName = request.FirstName;
        profile.LastName = request.LastName;
        profile.Phone = request.Phone;
        // profile.Dni no se actualiza - es inmutable

        await _unitOfWork.UserProfiles.UpdateAsync(profile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("Perfil actualizado exitosamente");
    }
}

