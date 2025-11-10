using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Users.Commands;

public class ResetUserPasswordCommand : IRequest<Result<string>>
{
    public Guid UserId { get; set; }
    public string NewPassword { get; set; } = string.Empty;
}

