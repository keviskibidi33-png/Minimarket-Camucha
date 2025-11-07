using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Auth.Queries;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Auth.Queries;

public class GetUserProfileStatusQueryHandler : IRequestHandler<GetUserProfileStatusQuery, Result<UserProfileStatusDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserProfileStatusQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserProfileStatusDto>> Handle(GetUserProfileStatusQuery request, CancellationToken cancellationToken)
    {
        var profile = await _unitOfWork.UserProfiles.FirstOrDefaultAsync(
            up => up.UserId == request.UserId, cancellationToken);

        var status = new UserProfileStatusDto
        {
            HasProfile = profile != null,
            ProfileCompleted = profile?.ProfileCompleted ?? false
        };

        return Result<UserProfileStatusDto>.Success(status);
    }
}

