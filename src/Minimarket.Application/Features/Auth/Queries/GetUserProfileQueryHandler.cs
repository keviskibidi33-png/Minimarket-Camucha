using MediatR;
using Microsoft.AspNetCore.Identity;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Auth.Queries;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, Result<UserProfileResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<IdentityUser<Guid>> _userManager;

    public GetUserProfileQueryHandler(IUnitOfWork unitOfWork, UserManager<IdentityUser<Guid>> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<Result<UserProfileResponse>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        // Obtener el email del usuario desde Identity
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        var email = user?.Email;

        var profile = await _unitOfWork.UserProfiles.FirstOrDefaultAsync(
            up => up.UserId == request.UserId, cancellationToken);

        if (profile == null)
        {
            return Result<UserProfileResponse>.Success(new UserProfileResponse
            {
                Email = email,
                ProfileCompleted = false
            });
        }

        return Result<UserProfileResponse>.Success(new UserProfileResponse
        {
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Dni = profile.Dni,
            Phone = profile.Phone,
            Email = email,
            ProfileCompleted = profile.ProfileCompleted
        });
    }
}

