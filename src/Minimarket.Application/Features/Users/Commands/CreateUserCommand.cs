using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Users.Queries;

namespace Minimarket.Application.Features.Users.Commands;

public class CreateUserCommand : IRequest<Result<UserDto>>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Password { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public bool EmailConfirmed { get; set; } = true;
}

