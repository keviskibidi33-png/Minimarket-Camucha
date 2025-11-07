using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Auth.Queries;

public class GetUserProfileQuery : IRequest<Result<UserProfileResponse>>
{
    public Guid UserId { get; set; }
}

public class UserProfileResponse
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Dni { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool ProfileCompleted { get; set; }
}

