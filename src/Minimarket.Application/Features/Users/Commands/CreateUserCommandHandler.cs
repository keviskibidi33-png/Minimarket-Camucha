using MediatR;
using Microsoft.AspNetCore.Identity;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Users.Queries;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Users.Commands;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(
        UserManager<IdentityUser<Guid>> userManager,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verificar si el email ya existe
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Result<UserDto>.Failure("El correo electrónico ya está registrado");
            }

            // Verificar si el DNI ya existe
            var existingProfile = await _unitOfWork.UserProfiles.FirstOrDefaultAsync(
                up => up.Dni == request.Dni, cancellationToken);
            if (existingProfile != null)
            {
                return Result<UserDto>.Failure("El DNI ya está registrado");
            }

            // Verificar si el DNI ya se usa como username
            existingUser = await _userManager.FindByNameAsync(request.Dni);
            if (existingUser != null)
            {
                return Result<UserDto>.Failure("El DNI ya está en uso");
            }

            // Crear nuevo usuario usando el DNI como username
            var user = new IdentityUser<Guid>
            {
                UserName = request.Dni,
                Email = request.Email,
                EmailConfirmed = request.EmailConfirmed
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return Result<UserDto>.Failure($"Error al crear usuario: {errors}");
            }

            // Asignar roles
            if (request.Roles.Any())
            {
                var roleResult = await _userManager.AddToRolesAsync(user, request.Roles);
                if (!roleResult.Succeeded)
                {
                    // Continuar aunque falle la asignación del rol
                }
            }

            // Crear perfil de usuario
            var profile = new UserProfile
            {
                UserId = user.Id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Dni = request.Dni,
                Phone = request.Phone,
                ProfileCompleted = true
            };

            await _unitOfWork.UserProfiles.AddAsync(profile, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Obtener roles asignados
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
            return Result<UserDto>.Failure($"Error al crear usuario: {ex.Message}");
        }
    }
}

