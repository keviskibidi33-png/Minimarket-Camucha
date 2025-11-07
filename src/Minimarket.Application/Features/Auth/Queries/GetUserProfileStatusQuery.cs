using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Auth.Queries;

public class GetUserProfileStatusQuery : IRequest<Result<UserProfileStatusDto>>
{
    public Guid UserId { get; set; }
}

public class UserProfileStatusDto
{
    public bool ProfileCompleted { get; set; }
    public bool HasProfile { get; set; }
}

