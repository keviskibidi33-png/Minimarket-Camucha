using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Users.Queries;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<List<UserDto>>>
{
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public GetAllUsersQueryHandler(
        UserManager<IdentityUser<Guid>> userManager,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Obtener todos los usuarios
            var users = await _userManager.Users
                .OrderByDescending(u => u.Id)
                .ToListAsync(cancellationToken);

            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                // Obtener roles del usuario
                var roles = await _userManager.GetRolesAsync(user);

                // Obtener perfil del usuario
                var profile = await _unitOfWork.UserProfiles.FirstOrDefaultAsync(
                    up => up.UserId == user.Id, cancellationToken);

                // Aplicar filtros
                if (!string.IsNullOrEmpty(request.RoleFilter) && request.RoleFilter != "all")
                {
                    if (!roles.Contains(request.RoleFilter))
                    {
                        continue;
                    }
                }

                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    var searchLower = request.SearchTerm.ToLower();
                    var fullName = $"{profile?.FirstName ?? ""} {profile?.LastName ?? ""}".Trim().ToLower();
                    var email = user.Email?.ToLower() ?? "";
                    var dni = profile?.Dni?.ToLower() ?? "";

                    if (!fullName.Contains(searchLower) && 
                        !email.Contains(searchLower) && 
                        !dni.Contains(searchLower))
                    {
                        continue;
                    }
                }

                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    FirstName = profile?.FirstName,
                    LastName = profile?.LastName,
                    Email = user.Email ?? string.Empty,
                    Dni = profile?.Dni,
                    Phone = profile?.Phone,
                    Roles = roles.ToList(),
                    CreatedAt = profile?.CreatedAt ?? DateTime.UtcNow,
                    ProfileCompleted = profile?.ProfileCompleted ?? false,
                    EmailConfirmed = user.EmailConfirmed
                });
            }

            // Aplicar paginación
            var totalCount = userDtos.Count;
            var paginatedUsers = userDtos
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return Result<List<UserDto>>.Success(paginatedUsers);
        }
        catch (Exception ex)
        {
            return Result<List<UserDto>>.Failure($"Error al obtener usuarios: {ex.Message}");
        }
    }
}

