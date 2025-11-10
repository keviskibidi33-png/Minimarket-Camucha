using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Users.Queries;

public class GetAllUsersQuery : IRequest<Result<List<UserDto>>>
{
    public string? SearchTerm { get; set; }
    public string? RoleFilter { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Dni { get; set; }
    public string? Phone { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public bool ProfileCompleted { get; set; }
    public bool EmailConfirmed { get; set; }
}

