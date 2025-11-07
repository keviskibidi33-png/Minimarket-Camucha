using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Auth.Commands;

public class UpdateProfileCommand : IRequest<Result<string>>
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    // DNI no se incluye porque no se puede modificar
}

