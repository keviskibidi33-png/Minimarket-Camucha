using MediatR;
using Microsoft.AspNetCore.Identity;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Users.Queries;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Users.Commands;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(
        UserManager<IdentityUser<Guid>> userManager,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return Result<UserDto>.Failure("Usuario no encontrado");
            }

            // Verificar si el email cambió y si ya existe
            if (user.Email != request.Email)
            {
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null && existingUser.Id != request.UserId)
                {
                    return Result<UserDto>.Failure("El correo electrónico ya está registrado");
                }
                user.Email = request.Email;
            }

            user.EmailConfirmed = request.EmailConfirmed;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                return Result<UserDto>.Failure($"Error al actualizar usuario: {errors}");
            }

            // Actualizar roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.Except(request.Roles).ToList();
            var rolesToAdd = request.Roles.Except(currentRoles).ToList();

            if (rolesToRemove.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }

            if (rolesToAdd.Any())
            {
                await _userManager.AddToRolesAsync(user, rolesToAdd);
            }

            // Actualizar perfil
            var profile = await _unitOfWork.UserProfiles.FirstOrDefaultAsync(
                up => up.UserId == request.UserId, cancellationToken);

            if (profile != null)
            {
                profile.FirstName = request.FirstName;
                profile.LastName = request.LastName;
                profile.Phone = request.Phone;
                await _unitOfWork.UserProfiles.UpdateAsync(profile, cancellationToken);
            }
            else
            {
                // Crear perfil si no existe
                profile = new Domain.Entities.UserProfile
                {
                    UserId = request.UserId,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Phone = request.Phone,
                    ProfileCompleted = true
                };
                await _unitOfWork.UserProfiles.AddAsync(profile, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Obtener roles actualizados
            var roles = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto
            {
                Id = user.Id,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                Email = user.Email ?? string.Empty,
                Dni = profile.Dni,
                Phone = profile.Phone,
                Roles = roles.ToList(),
                CreatedAt = DateTime.UtcNow,
                ProfileCompleted = profile.ProfileCompleted,
                EmailConfirmed = user.EmailConfirmed
            };

            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Error al actualizar usuario: {ex.Message}");
        }
    }
}

