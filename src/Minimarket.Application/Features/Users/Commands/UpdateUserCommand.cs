using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Users.Queries;

namespace Minimarket.Application.Features.Users.Commands;

public class UpdateUserCommand : IRequest<Result<UserDto>>
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool EmailConfirmed { get; set; }
}

